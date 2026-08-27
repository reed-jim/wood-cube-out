using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using UnityEngine;

namespace BusFlow
{
    // Phép co/đặt map + bãi đỗ cho vừa khung camera. TÁCH RIÊNG để runtime (BusFlowGameManager) và
    // editor (SimulateMap) dùng CHUNG đúng 1 công thức -> preview khớp lúc play.
    public static class BusBoardFit
    {
        public struct Fit
        {
            public Vector3 origin;
            public float scale;
        }

        // Co map (khách + spline) vừa vùng camera. tiles là lưới hành khách (LevelItem.tiles).
        public static Fit ComputeBoard(
            Camera cam,
            List<AntsFlow.BaseTile> tiles,
            Vector2 spacing,
            Vector3 fallbackOrigin,
            float fitWidthRatio,
            float fitHeightRatio,
            float fitCenterViewportY)
        {
            Fit fit = new Fit { origin = fallbackOrigin, scale = 1f };

            if (cam == null || !cam.orthographic || tiles == null || tiles.Count == 0)
            {
                return fit;
            }

            float minCol = tiles.Min(t => t.position.x);
            float maxCol = tiles.Max(t => t.position.x);
            float minRow = tiles.Min(t => t.position.y);
            float maxRow = tiles.Max(t => t.position.y);

            float mapWidth = Mathf.Max(0.001f, (maxCol - minCol) * spacing.x);
            float mapHeight = Mathf.Max(0.001f, (maxRow - minRow) * spacing.y);

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float availableWidth = 2f * halfW * fitWidthRatio;
            float availableHeight = 2f * halfH * fitHeightRatio;

            fit.scale = Mathf.Min(availableWidth / mapWidth, availableHeight / mapHeight);

            // Tâm map trong hệ BusTileToLocal (x đã căn giữa = 0; y giữa map + BusBoardOriginY).
            float mapCenterYLocal = mapHeight * 0.5f + AntsFlow.LevelItem.BusBoardOriginY;

            float camBottomY = cam.transform.position.y - halfH;
            float targetCenterY = camBottomY + fitCenterViewportY * (2f * halfH);

            fit.origin = new Vector3(
                cam.transform.position.x,
                targetCenterY - fit.scale * mapCenterYLocal,
                fallbackOrigin.z);

            return fit;
        }

        // Đặt bãi đỗ ở đáy khung + co cho vừa bề rộng.
        public static Fit ComputeGarage(
            Camera cam,
            int numColumn,
            Vector2 slotSpacing,
            Vector3 fallbackOrigin,
            float garageWidthRatio,
            float garageViewportY)
        {
            Fit fit = new Fit { origin = fallbackOrigin, scale = 1f };

            if (cam == null || !cam.orthographic)
            {
                return fit;
            }

            int cols = Mathf.Max(1, numColumn);
            float gridWidth = Mathf.Max(0.001f, cols * slotSpacing.x);

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            fit.scale = (2f * halfW * garageWidthRatio) / gridWidth;

            float camBottomY = cam.transform.position.y - halfH;

            fit.origin = new Vector3(
                cam.transform.position.x,
                camBottomY + garageViewportY * (2f * halfH),
                0f);

            return fit;
        }
    }
}
