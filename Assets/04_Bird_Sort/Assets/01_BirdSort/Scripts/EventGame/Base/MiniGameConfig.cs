using System;

[Serializable]
public class MiniGameConfig
{
    public int version;
    public int version_accept;
    public int num_watch_video_buy_egg;//Số lần xem video để nhận Egg trong ngày
    public int num_turn_free;//Số lần free để chơi mini game trong ngày

    public bool is_active;
    public string time_play;
    public int min_level;
    public int time_delay_show;

    public bool IsActive =>
         is_active
         &&
         version_accept == MiniGameController.Version_Except;

    public DateTime start_date;
    public DateTime end_date;
    public string str_start_date;
    public string str_end_date;
}
 