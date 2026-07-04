#import "FsnAdIOS.h"
#import <objc/runtime.h>

// Biến toàn cục lưu trữ các con trỏ hàm Function Pointers gửi từ Unity C# sang.
// Bản mới truyền thêm requestId để route callback đúng FsnAd instance.
typedef void (*IOSLoadingCompletedDelegate)(int requestId, int errorCode, const char* errorMessage);
typedef void (*IOSLoadingStartedDelegate)(int requestId);
typedef void (*IOSAdPaidDelegate)(int requestId, const char* adSource, const char* adUnitId, long long valueMicros, const char* currencyCode);
typedef void (*IOSAdCompletedDelegate)(int requestId, const char* errorMessage);

static IOSLoadingCompletedDelegate _unityOnLoadingCompleted = NULL;
static IOSLoadingStartedDelegate _unityOnLoadingStarted = NULL;
static IOSAdPaidDelegate _unityOnAdPaid = NULL;
static IOSAdCompletedDelegate _unityOnAdCompleted = NULL;

static const void *FsnAdLoaderRequestIdKey = &FsnAdLoaderRequestIdKey;
static const void *FsnVideoControllerRequestIdKey = &FsnVideoControllerRequestIdKey;
static const void *FsnContainerIsCollapsibleKey = &FsnContainerIsCollapsibleKey;

static const NSInteger FsnCloseButtonTag = 91001;
static const NSInteger FsnCountDownLabelTag = 91002;
static const NSInteger FsnAdLabelTag = 91003;
static const NSInteger FsnOpenStoreButtonTag = 91004;
static const NSInteger FsnFakeCloseButtonTag = 91005;
static const NSInteger FsnCollapsibleSubHeadlineTag = 91006;
static const NSInteger FsnNativeContainerTag = 91099;
static const NSInteger FsnNativeAdViewTag = 91100;
static const NSInteger FsnRealCtaButtonTag = 91007;

// Full-screen transparent/opaque blocker.
// Purpose: even if the native ad layout has empty/clear areas, touches must not fall through to Unity.
@interface FsnTouchBlockerView : UIView
@end

@implementation FsnTouchBlockerView

- (UIView *)fsnFindVisibleSubviewWithTag:(NSInteger)tag inView:(UIView *)view {
    if (!view) {
        return nil;
    }

    for (UIView *subview in [[view.subviews reverseObjectEnumerator] allObjects]) {
        if (subview.tag == tag &&
            !subview.hidden &&
            subview.alpha >= 0.01 &&
            subview.userInteractionEnabled) {
            return subview;
        }

        UIView *found = [self fsnFindVisibleSubviewWithTag:tag inView:subview];
        if (found) {
            return found;
        }
    }

    return nil;
}


- (UIView *)hitTest:(CGPoint)point withEvent:(UIEvent *)event {
    if (self.hidden || self.alpha < 0.01 || !self.userInteractionEnabled) {
        return nil;
    }

    if (![self pointInside:point withEvent:event]) {
        return nil;
    }

    // 1. Nút close luôn được ưu tiên bắt touch, kể cả mở rộng vùng bấm.
    UIView *closeButton = [self fsnFindVisibleSubviewWithTag:FsnCloseButtonTag inView:self];
    if (closeButton) {
        CGRect closeFrameInContainer = [closeButton convertRect:closeButton.bounds toView:self];
        CGRect expandedHitFrame = CGRectInset(closeFrameInContainer, -16.0, -16.0);

        if (CGRectContainsPoint(expandedHitFrame, point)) {
            CGPoint pointInClose = [closeButton convertPoint:point fromView:self];
            UIView *hitClose = [closeButton hitTest:pointInClose withEvent:event];

            NSLog(@"[FsnAdIOS][HitTest] Force route touch to close button. point=%@ | closeFrame=%@",
                  NSStringFromCGPoint(point),
                  NSStringFromCGRect(closeFrameInContainer));

            return hitClose ?: closeButton;
        }
    }

    UIView *hitView = [super hitTest:point withEvent:event];

    NSNumber *isCollapsibleNum = objc_getAssociatedObject(self, FsnContainerIsCollapsibleKey);
    BOOL isCollapsible = isCollapsibleNum ? [isCollapsibleNum boolValue] : NO;

    // 2. Với full-screen/native thường: vẫn block Unity như cũ.
    if (!isCollapsible) {
        return hitView ?: self;
    }

    // 3. Với collapsible:
    // Nếu chạm vào nền trong suốt/container rỗng -> cho touch xuyên xuống Unity.
    if (!hitView || hitView == self) {
        NSLog(@"[FsnAdIOS][HitTest] Collapsible passthrough empty container touch. point=%@",
              NSStringFromCGPoint(point));
        return nil;
    }

    UIView *nativeAdView = [self fsnFindVisibleSubviewWithTag:FsnNativeAdViewTag inView:self];

    // 4. Nếu hit vào chính GADNativeAdView nhưng không trúng asset con nào
    // thì coi như vùng nền/blank, không phải click ads.
    // Cho touch xuyên xuống Unity để game vẫn thao tác được.
    if (nativeAdView && hitView == nativeAdView) {
        NSLog(@"[FsnAdIOS][HitTest] Collapsible passthrough blank nativeAdView touch. point=%@",
              NSStringFromCGPoint(point));
        return nil;
    }

    // 5. Nếu hit vào media/icon/headline/body/CTA/close thì giữ lại cho ads xử lý.
    return hitView;
}

@end

@implementation FsnAdStateIOS

- (instancetype)init {
    self = [super init];

    if (self) {
        _requestId = 0;
        _nativeType = 0;

        _adUnitId = @"";
        _countDownSec = 5;
        _delayForCountDown = 0;
        _mediaAspectRatio = GADMediaAspectRatioAny;

        _nativeAd = nil;
        _adLoader = nil;

        _isLoading = NO;
        _lastLoadTime = 0;
        _adContainerView = nil;
        _countDownTimer = nil;
        _currentRemainingSec = 0;
		_shouldCloseAfterAdClick = NO;
    }

    return self;
}

@end

@interface FsnAdIOS () <GADVideoControllerDelegate>

@property (nonatomic, strong) NSMutableDictionary<NSNumber*, FsnAdStateIOS*> *nativeStates;

@end

@implementation FsnAdIOS

+ (instancetype)sharedInstance {
    static FsnAdIOS *sharedInstance = nil;
    static dispatch_once_t onceToken;

    dispatch_once(&onceToken, ^{
        sharedInstance = [[FsnAdIOS alloc] init];
    });

    return sharedInstance;
}

- (instancetype)init {
    self = [super init];

    if (self) {
        _nativeStates = [[NSMutableDictionary alloc] init];

        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(fsnApplicationDidBecomeActive:)
                                                     name:UIApplicationDidBecomeActiveNotification
                                                   object:nil];
    }

    return self;
}

- (void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (FsnAdStateIOS *)stateForRequestId:(int)requestId {
    return _nativeStates[@(requestId)];
}

- (void)fsnApplicationDidBecomeActive:(NSNotification *)notification {
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(0.3 * NSEC_PER_SEC)),
                   dispatch_get_main_queue(), ^{
        NSArray<NSNumber *> *keys = [self.nativeStates.allKeys copy];

        for (NSNumber *key in keys) {
            FsnAdStateIOS *state = self.nativeStates[key];

            if (!state || !state.shouldCloseAfterAdClick || !state.adContainerView) {
                continue;
            }

            NSLog(@"[FsnAdIOS][ReturnFromClick] App active again. Close native ad. RequestId=%d",
                  state.requestId);

            state.shouldCloseAfterAdClick = NO;
            [self dismissNativeAdContainer:state errorMessage:@""];
        }
    });
}

- (NSString *)layoutNameForType:(int)layoutType {
    switch (layoutType) {
        case 0:
            return @"Style1-AndroidBottomCard";

        case 1:
            return @"Style2-AndroidTopDarkCentered";

        case 2:
            return @"Style3-AndroidMediaWeightBottomInfo";

        case 3:
            return @"Style4-AndroidCenteredCard";

        default:
            return @"Unknown-FallbackStyle3";
    }
}

- (NSString *)nativeTypeNameForType:(int)type {
    switch (type) {
        case FsnAdTypeNormal:
            return @"Normal";

        case FsnAdTypeFakeClose:
            return @"FakeClose";

        case FsnAdTypeCountdown:
            return @"Countdown";

        case FsnAdTypeCollapsible:
            return @"Collapsible";

        default:
            return @"Unknown";
    }
}

- (NSString *)mediaRatioNameForRatio:(GADMediaAspectRatio)ratio {
    switch (ratio) {
        case GADMediaAspectRatioLandscape:
            return @"Landscape";

        case GADMediaAspectRatioPortrait:
            return @"Portrait";

        case GADMediaAspectRatioSquare:
            return @"Square";

        case GADMediaAspectRatioAny:
        default:
            return @"Any";
    }
}

- (NSString *)stringFromCGRectSafe:(CGRect)rect {
    return NSStringFromCGRect(rect);
}

- (void)logMediaContent:(GADMediaContent *)mediaContent
              requestId:(int)requestId
                  phase:(NSString *)phase {

    if (!mediaContent) {
        NSLog(@"[FsnAdIOS][Media] %@ RequestId: %d | mediaContent = nil", phase, requestId);
        return;
    }

    NSLog(@"[FsnAdIOS][Media] %@ RequestId: %d | hasVideo=%@ | aspectRatio=%.4f | videoController=%@",
          phase,
          requestId,
          mediaContent.hasVideoContent ? @"YES" : @"NO",
          mediaContent.aspectRatio,
          mediaContent.videoController);
}

- (void)initNativeWithRequestId:(int)requestId
                           type:(int)type
                     mediaRatio:(int)ratio
                       adUnitId:(NSString*)adUnitId
                   countDownSec:(int)countDownSec
              delayForCountDown:(long)delay {

    if (requestId <= 0) {
        NSLog(@"[FsnAdIOS] InitNative failed. Invalid requestId: %d", requestId);
        return;
    }

    FsnAdStateIOS *state = _nativeStates[@(requestId)];
    if (!state) {
        state = [[FsnAdStateIOS alloc] init];
        _nativeStates[@(requestId)] = state;
    }

    state.requestId = requestId;
    state.nativeType = type;
    state.adUnitId = adUnitId ?: @"";
    state.countDownSec = countDownSec;
    state.delayForCountDown = delay;

    switch (ratio) {
        case 1:
            state.mediaAspectRatio = GADMediaAspectRatioLandscape;
            break;

        case 2:
            state.mediaAspectRatio = GADMediaAspectRatioPortrait;
            break;

        case 3:
            state.mediaAspectRatio = GADMediaAspectRatioSquare;
            break;

        default:
            state.mediaAspectRatio = GADMediaAspectRatioAny;
            break;
    }

    NSLog(@"[FsnAdIOS] InitNative RequestId: %d, Type: %d, UnitId: %@, CountDown: %d",
          requestId,
          type,
          state.adUnitId,
          countDownSec);
}

- (void)loadNativeAdWithRequestId:(int)requestId
                       mediaRatio:(int)ratio {

    FsnAdStateIOS *state = [self stateForRequestId:requestId];

    if (!state) {
        NSLog(@"[FsnAdIOS] LoadNativeAd failed. State not found. RequestId: %d", requestId);

        if (_unityOnLoadingCompleted) {
            _unityOnLoadingCompleted(requestId, -1000, "Native state not found");
        }

        return;
    }

    if (state.isLoading) {
        NSLog(@"[FsnAdIOS] LoadNativeAd ignored. Already loading. RequestId: %d, Type: %d",
              requestId,
              state.nativeType);
        return;
    }

    if (state.adUnitId.length <= 0) {
        NSLog(@"[FsnAdIOS] LoadNativeAd failed. Empty adUnitId. RequestId: %d, Type: %d",
              requestId,
              state.nativeType);

        if (_unityOnLoadingCompleted) {
            _unityOnLoadingCompleted(requestId, -1002, "AdUnitId is empty");
        }

        return;
    }

    switch (ratio) {
        case 1:
            state.mediaAspectRatio = GADMediaAspectRatioLandscape;
            break;

        case 2:
            state.mediaAspectRatio = GADMediaAspectRatioPortrait;
            break;

        case 3:
            state.mediaAspectRatio = GADMediaAspectRatioSquare;
            break;

        default:
            state.mediaAspectRatio = GADMediaAspectRatioAny;
            break;
    }

    if (_unityOnLoadingStarted) {
        _unityOnLoadingStarted(requestId);
    }

    state.isLoading = YES;

    UIViewController *rootVC = UnityGetGLViewController();

    if (!rootVC) {
        state.isLoading = NO;

        NSLog(@"[FsnAdIOS] LoadNativeAd failed. Unity rootViewController is null. RequestId: %d, Type: %d",
              requestId,
              state.nativeType);

        if (_unityOnLoadingCompleted) {
            _unityOnLoadingCompleted(requestId, -1001, "Unity rootViewController is null");
        }

        return;
    }

    GADVideoOptions *videoOptions = [[GADVideoOptions alloc] init];
	videoOptions.startMuted = (state.nativeType == FsnAdTypeCollapsible);
	videoOptions.customControlsRequested = YES;
	videoOptions.clickToExpandRequested = NO;

    GADNativeAdMediaAdLoaderOptions *mediaOptions = [[GADNativeAdMediaAdLoaderOptions alloc] init];
    mediaOptions.mediaAspectRatio = state.mediaAspectRatio;

    GADAdLoader *adLoader = [[GADAdLoader alloc] initWithAdUnitID:state.adUnitId
                                               rootViewController:rootVC
                                                          adTypes:@[GADAdLoaderAdTypeNative]
                                                          options:@[videoOptions, mediaOptions]];

    adLoader.delegate = self;

    objc_setAssociatedObject(
        adLoader,
        FsnAdLoaderRequestIdKey,
        @(requestId),
        OBJC_ASSOCIATION_RETAIN_NONATOMIC
    );

    // Giữ strong reference để delegate callback không bị mất.
    state.adLoader = adLoader;

    GADRequest *request = [GADRequest request];
    [state.adLoader loadRequest:request];

    NSLog(@"[FsnAdIOS] Start Loading Native Ad RequestId: %d, Type: %d, Unit: %@",
          requestId,
          state.nativeType,
          state.adUnitId);
}

- (BOOL)isNativeAdReadyWithRequestId:(int)requestId {
    FsnAdStateIOS *state = [self stateForRequestId:requestId];
    return state != nil && state.nativeAd != nil;
}

- (void)hideNativeAdWithRequestId:(int)requestId {
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"[FsnAdIOS][Hide] Force hide native. RequestId=%d", requestId);

        FsnAdStateIOS *state = [self stateForRequestId:requestId];

        if (state && state.adContainerView) {
            [self fsnRemoveContainerView:state.adContainerView reason:@"hide-target-container"];
        }

        // Backup: remove any orphan FSN native views that may have escaped state tracking.
        [self fsnForceRemoveAllFsnNativeViewsFromRootOnly];

        if (state) {
            [self fsnClearNativeState:state];
            [self.nativeStates removeObjectForKey:@(requestId)];
        }

        if (_unityOnAdCompleted) {
            _unityOnAdCompleted(requestId, "");
        }
    });
}

- (UIView *)fsnTopParentViewFromRootViewController:(UIViewController *)rootVC {
    if (rootVC.view.window) {
        return rootVC.view.window;
    }

    if (@available(iOS 13.0, *)) {
        for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
            if (scene.activationState != UISceneActivationStateForegroundActive) {
                continue;
            }

            if (![scene isKindOfClass:[UIWindowScene class]]) {
                continue;
            }

            UIWindowScene *windowScene = (UIWindowScene *)scene;
            for (UIWindow *window in windowScene.windows) {
                if (window.isKeyWindow) {
                    return window;
                }
            }

            if (windowScene.windows.count > 0) {
                return windowScene.windows.firstObject;
            }
        }
    }

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
    if (UIApplication.sharedApplication.keyWindow) {
        return UIApplication.sharedApplication.keyWindow;
    }
#pragma clang diagnostic pop

    return rootVC.view;
}

// MARK: - GADNativeAdLoaderDelegate Implementation

- (void)adLoader:(GADAdLoader *)adLoader didReceiveNativeAd:(GADNativeAd *)nativeAd {
    NSNumber *requestIdNum = objc_getAssociatedObject(adLoader, FsnAdLoaderRequestIdKey);
    int requestId = requestIdNum ? [requestIdNum intValue] : 0;

    FsnAdStateIOS *state = [self stateForRequestId:requestId];

    if (!state) {
        NSLog(@"[FsnAdIOS] didReceiveNativeAd ignored. State not found. RequestId: %d", requestId);
        return;
    }

    state.isLoading = NO;
    state.adLoader = nil;

    state.nativeAd = nativeAd;
    state.lastLoadTime = [[NSDate date] timeIntervalSince1970];

    nativeAd.delegate = self;

    if (nativeAd.mediaContent.videoController) {
        nativeAd.mediaContent.videoController.delegate = self;
        objc_setAssociatedObject(
            nativeAd.mediaContent.videoController,
            FsnVideoControllerRequestIdKey,
            @(requestId),
            OBJC_ASSOCIATION_RETAIN_NONATOMIC
        );
    }

    [self logMediaContent:nativeAd.mediaContent
                requestId:requestId
                    phase:@"didReceiveNativeAd"];

    NSString *currentAdUnitId = state.adUnitId ?: @"";
    int capturedRequestId = requestId;

    nativeAd.paidEventHandler = ^(GADAdValue * _Nonnull adValue) {
        if (_unityOnAdPaid) {
            const char* adSource = "AdMob";
            const char* adUnitId = [currentAdUnitId UTF8String] ? [currentAdUnitId UTF8String] : "";
            long long valueMicros = adValue.value.longLongValue;
            const char* currencyCode = [adValue.currencyCode UTF8String] ? [adValue.currencyCode UTF8String] : "USD";

            _unityOnAdPaid(capturedRequestId, adSource, adUnitId, valueMicros, currencyCode);
        }
    };

    NSLog(@"[FsnAdIOS] Load Native Ad Success. RequestId: %d, Type: %d(%@), Ratio=%@, HasVideo=%@, Aspect=%.4f",
          requestId,
          state.nativeType,
          [self nativeTypeNameForType:state.nativeType],
          [self mediaRatioNameForRatio:state.mediaAspectRatio],
          nativeAd.mediaContent.hasVideoContent ? @"YES" : @"NO",
          nativeAd.mediaContent.aspectRatio);

    if (_unityOnLoadingCompleted) {
        _unityOnLoadingCompleted(requestId, 0, "");
    }
}

- (void)adLoader:(GADAdLoader *)adLoader didFailToReceiveAdWithError:(NSError *)error {
    NSNumber *requestIdNum = objc_getAssociatedObject(adLoader, FsnAdLoaderRequestIdKey);
    int requestId = requestIdNum ? [requestIdNum intValue] : 0;

    FsnAdStateIOS *state = [self stateForRequestId:requestId];

    if (!state) {
        NSLog(@"[FsnAdIOS] didFailToReceiveAd ignored. State not found. RequestId: %d", requestId);

        if (_unityOnLoadingCompleted) {
            NSString *fallbackMessage = error.localizedDescription ?: @"Unknown AdMob native load error";
            _unityOnLoadingCompleted(requestId, (int)error.code, [fallbackMessage UTF8String]);
        }

        return;
    }

    state.isLoading = NO;
    state.adLoader = nil;
    state.nativeAd = nil;

    NSString *errorMessage = error.localizedDescription ?: @"Unknown AdMob native load error";

    NSLog(@"[FsnAdIOS] Load Native Ad Failed. RequestId: %d, Type: %d, Error: %@",
          requestId,
          state.nativeType,
          errorMessage);

    if (_unityOnLoadingCompleted) {
        _unityOnLoadingCompleted(requestId, (int)error.code, [errorMessage UTF8String]);
    }
}

// MARK: - HÀM HIỂN THỊ VÀ DỌN DẸP VIEW

- (void)showNativeAdWithRequestId:(int)requestId
                       layoutType:(int)layoutType
              overlayOpenStorePos:(int)openStorePos
                  overlayClosePos:(int)closePos {

    FsnAdStateIOS *state = [self stateForRequestId:requestId];

    if (!state || !state.nativeAd) {
        if (_unityOnAdCompleted) {
            _unityOnAdCompleted(requestId, "Ad not ready or null");
        }

        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"[FsnAdIOS][Show] BEGIN RequestId: %d | NativeType=%d(%@) | LayoutType=%d(%@) | OpenStorePos=%d | ClosePos=%d | Ratio=%@",
              requestId,
              state.nativeType,
              [self nativeTypeNameForType:state.nativeType],
              layoutType,
              [self layoutNameForType:layoutType],
              openStorePos,
              closePos,
              [self mediaRatioNameForRatio:state.mediaAspectRatio]);

        [self logMediaContent:state.nativeAd.mediaContent
                    requestId:requestId
                        phase:@"before-show"];

        UIViewController *rootVC = UnityGetGLViewController();

        if (!rootVC) {
            if (_unityOnAdCompleted) {
                _unityOnAdCompleted(requestId, "Unity rootViewController is null");
            }

            return;
        }

        UIView *targetParentView = [self fsnTopParentViewFromRootViewController:rootVC];
        CGRect parentBounds = targetParentView.bounds;
        BOOL isCollapsible = (state.nativeType == FsnAdTypeCollapsible);
		
		if (isCollapsible) {
			NSLog(@"[FsnAdIOS][Show] Collapsible requested. Remove old visible FSN native containers before showing new one, preserve current requestId=%d.", requestId);
			[self fsnForceRemoveAllFsnNativeViewsFromRootOnly];
			[self fsnClearVisibleNativeStatesExceptRequestId:requestId];
		}

        CGFloat collapsibleHeight = MIN(350.0, parentBounds.size.height);
        CGRect containerFrame = parentBounds;

        if (isCollapsible) {
            containerFrame = CGRectMake(0.0,
                                        parentBounds.size.height - collapsibleHeight,
                                        parentBounds.size.width,
                                        collapsibleHeight);
        }

        FsnTouchBlockerView *container = [[FsnTouchBlockerView alloc] initWithFrame:containerFrame];
		container.tag = FsnNativeContainerTag;

		objc_setAssociatedObject(
			container,
			FsnContainerIsCollapsibleKey,
			@(isCollapsible),
			OBJC_ASSOCIATION_RETAIN_NONATOMIC
		);

		container.userInteractionEnabled = YES;
        container.multipleTouchEnabled = YES;
        container.exclusiveTouch = YES;
        container.autoresizingMask = isCollapsible
            ? (UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleTopMargin)
            : (UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight);
        container.backgroundColor = isCollapsible ? [UIColor clearColor] : [UIColor blackColor];
        container.layer.zPosition = CGFLOAT_MAX;

        CGRect layoutBounds = container.bounds;

        GADNativeAdView *adView = [[GADNativeAdView alloc] initWithFrame:layoutBounds];
		adView.tag = FsnNativeAdViewTag;
		adView.userInteractionEnabled = YES;
		adView.multipleTouchEnabled = YES;
		adView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;

        [container addSubview:adView];

        UIImageView *iconView = [[UIImageView alloc] init];
        iconView.layer.cornerRadius = 8;
        iconView.clipsToBounds = YES;

        UILabel *headlineLabel = [[UILabel alloc] init];
        headlineLabel.textColor = [UIColor whiteColor];
        headlineLabel.font = [UIFont boldSystemFontOfSize:17];
        headlineLabel.numberOfLines = 2;

        UILabel *bodyLabel = [[UILabel alloc] init];
        bodyLabel.textColor = [UIColor lightGrayColor];
        bodyLabel.font = [UIFont systemFontOfSize:14];
        bodyLabel.numberOfLines = 2;

        UIButton *ctaButton = [UIButton buttonWithType:UIButtonTypeCustom];
        ctaButton.backgroundColor = [UIColor colorWithRed:0.0 green:0.5 blue:1.0 alpha:1.0];
        ctaButton.layer.cornerRadius = 8;
        ctaButton.titleLabel.font = [UIFont boldSystemFontOfSize:16];
		ctaButton.tag = FsnRealCtaButtonTag;
        [ctaButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];

        GADMediaView *mediaView = [[GADMediaView alloc] init];

        UIButton *closeButton = [UIButton buttonWithType:UIButtonTypeCustom];
		closeButton.tag = FsnCloseButtonTag;

		NSString *closeImageName = isCollapsible ? @"fsn_collapsible_close_ic1" : @"fsn_ad_close_ic";
		UIImage *closeImage = [self fsnLoadImageNamed:closeImageName];
		
		if (closeImage) {
			[closeButton setImage:closeImage forState:UIControlStateNormal];
			closeButton.backgroundColor = [UIColor clearColor];
		} else {
			[closeButton setTitle:@"X" forState:UIControlStateNormal];
			closeButton.titleLabel.font = [UIFont boldSystemFontOfSize:18.0];
			[closeButton setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
			closeButton.backgroundColor = [[UIColor whiteColor] colorWithAlphaComponent:0.8];
		}
	
		closeButton.imageView.contentMode = UIViewContentModeScaleAspectFit;
		closeButton.contentEdgeInsets = isCollapsible ? UIEdgeInsetsMake(7, 7, 7, 7) : UIEdgeInsetsMake(5, 5, 5, 5);
		closeButton.clipsToBounds = YES;
		closeButton.layer.cornerRadius = isCollapsible ? 18.0 : 17.5;

        objc_setAssociatedObject(
            closeButton,
            (const void *)@"FsnCloseActionRequestId",
            @(requestId),
            OBJC_ASSOCIATION_RETAIN_NONATOMIC
        );

        [closeButton addTarget:self
                        action:@selector(closeButtonTapped:)
              forControlEvents:UIControlEventTouchUpInside];

        UILabel *countDownLabel = [[UILabel alloc] init];
        countDownLabel.tag = FsnCountDownLabelTag;
        countDownLabel.textColor = [UIColor whiteColor];
        countDownLabel.font = [UIFont systemFontOfSize:12];
        countDownLabel.textAlignment = NSTextAlignmentCenter;
        countDownLabel.backgroundColor = [[UIColor blackColor] colorWithAlphaComponent:0.6];
        countDownLabel.layer.cornerRadius = 4;
        countDownLabel.clipsToBounds = YES;
        countDownLabel.hidden = YES;
		UIImageView *openStoreButton = [[UIImageView alloc] init];
		openStoreButton.tag = FsnOpenStoreButtonTag;
		openStoreButton.image = [self fsnLoadImageNamed:@"fsn_collapsible_countdown_ic"];
		openStoreButton.contentMode = UIViewContentModeScaleAspectFit;
		openStoreButton.clipsToBounds = YES;
		openStoreButton.hidden = YES;
		openStoreButton.userInteractionEnabled = NO;

		// Để giống Android: đây là overlay giả, không phải nút close thật.
		// Không tự dismiss. Nếu muốn click xuyên vào ad, để NO.
		openStoreButton.userInteractionEnabled = NO;

		UIImageView *fakeCloseButton = [[UIImageView alloc] init];
		fakeCloseButton.tag = FsnFakeCloseButtonTag;
		fakeCloseButton.image = [self fsnLoadImageNamed:@"fsn_ad_close_ic_fake"];
		fakeCloseButton.contentMode = UIViewContentModeScaleAspectFit;
		fakeCloseButton.clipsToBounds = YES;
		fakeCloseButton.hidden = YES;
		// Fake close chỉ để đánh lừa thị giác giống Android,
		// không phải close thật.
		fakeCloseButton.userInteractionEnabled = NO;

        headlineLabel.text = state.nativeAd.headline;
        adView.headlineView = headlineLabel;
        [adView addSubview:headlineLabel];

        if (state.nativeAd.body) {
            bodyLabel.text = state.nativeAd.body;
            adView.bodyView = bodyLabel;
            [adView addSubview:bodyLabel];
        }

        if (state.nativeAd.callToAction) {
            [ctaButton setTitle:state.nativeAd.callToAction forState:UIControlStateNormal];
            // Required for UIButton assets so Google Mobile Ads SDK can receive and process touches.
            ctaButton.userInteractionEnabled = NO;
            adView.callToActionView = ctaButton;
            [adView addSubview:ctaButton];
        }

        if (state.nativeAd.icon) {
            iconView.image = state.nativeAd.icon.image;
            adView.iconView = iconView;
            [adView addSubview:iconView];
        }

        mediaView.contentMode = UIViewContentModeScaleAspectFit;
		mediaView.clipsToBounds = YES;
		mediaView.layer.masksToBounds = YES;
		mediaView.backgroundColor = [UIColor clearColor];
		
        adView.mediaView = mediaView;
        [adView addSubview:mediaView];

        if (state.nativeAd.mediaContent.videoController) {
            state.nativeAd.mediaContent.videoController.delegate = self;
            objc_setAssociatedObject(
                state.nativeAd.mediaContent.videoController,
                FsnVideoControllerRequestIdKey,
                @(requestId),
                OBJC_ASSOCIATION_RETAIN_NONATOMIC
            );
        }

        // Close thật / countdown thật nằm ngoài GADNativeAdView.
        // Lý do: GADNativeAdView là vùng SDK dùng để bắt click ads.
        // Nếu close thật nằm trong đó, một số creative lỗi StoreKit có thể nuốt touch close thành click ads.
        [container addSubview:closeButton];
		[container addSubview:countDownLabel];

        // OpenStore/FakeClose vẫn là vùng click ads giả lập CTA nên giữ trong GADNativeAdView.
		[adView addSubview:openStoreButton];
		[adView addSubview:fakeCloseButton];

        if (isCollapsible) {
            [self applyLayoutCollapsible:container
                                  adView:adView
                                    icon:iconView
                                headline:headlineLabel
                                    body:bodyLabel
                                     cta:ctaButton
                                   media:mediaView
                             closeButton:closeButton
                                  bounds:layoutBounds];
        } else {
        switch (layoutType) {
            case 0:
                [self applyLayoutStyle1:container
                                  adView:adView
                                    icon:iconView
                                headline:headlineLabel
                                    body:bodyLabel
                                     cta:ctaButton
                                   media:mediaView
                                  bounds:layoutBounds];
                break;

            case 1:
                [self applyLayoutStyle2:container
                                  adView:adView
                                    icon:iconView
                                headline:headlineLabel
                                    body:bodyLabel
                                     cta:ctaButton
                                   media:mediaView
                                  bounds:layoutBounds];
                break;

            case 2:
                [self applyLayoutStyle3:container
                                  adView:adView
                                    icon:iconView
                                headline:headlineLabel
                                    body:bodyLabel
                                     cta:ctaButton
                                   media:mediaView
                                  bounds:layoutBounds];
                break;

            case 3:
                [self applyLayoutStyle4:container
                                  adView:adView
                                    icon:iconView
                                headline:headlineLabel
                                    body:bodyLabel
                                     cta:ctaButton
                                   media:mediaView
                                  bounds:layoutBounds];
                break;

            default:
                [self applyLayoutStyle3:container
                                  adView:adView
                                    icon:iconView
                                headline:headlineLabel
                                    body:bodyLabel
                                     cta:ctaButton
                                   media:mediaView
                                  bounds:layoutBounds];
                break;
        }
        }

        if (!isCollapsible) {
            [self applyClosePosition:closeButton
                               label:countDownLabel
                            position:closePos
                        parentBounds:container.bounds];
        }

        NSLog(@"[FsnAdIOS][Layout] Applied RequestId: %d | LayoutType=%d(%@) | Container=%@ | AdView=%@ | MediaFrame=%@ | HasVideo=%@ | Aspect=%.4f",
              requestId,
              layoutType,
              [self layoutNameForType:layoutType],
              [self stringFromCGRectSafe:container.frame],
              [self stringFromCGRectSafe:adView.frame],
              [self stringFromCGRectSafe:mediaView.frame],
              state.nativeAd.mediaContent.hasVideoContent ? @"YES" : @"NO",
              state.nativeAd.mediaContent.aspectRatio);

        // Nên set nativeAd sau khi asset views đã gán xong.
		GADNativeAd *renderedNativeAd = state.nativeAd;

		// 1. Add view lên window trước.
		state.adContainerView = container;
		[targetParentView addSubview:container];
		[targetParentView bringSubviewToFront:container];

		// 2. Ép layout chạy thật.
		[container setNeedsLayout];
		[container layoutIfNeeded];

		[adView setNeedsLayout];
		[adView layoutIfNeeded];

		[mediaView setNeedsLayout];
		[mediaView layoutIfNeeded];

		NSLog(@"[FsnAdIOS][Show] ADDED_TO_WINDOW RequestId: %d | LayoutType=%d(%@) | MediaFrame=%@ | HasVideo=%@ | Duration=%.3f | CurrentTime=%.3f | Aspect=%.4f",
			  requestId,
			  layoutType,
			  [self layoutNameForType:layoutType],
			  [self stringFromCGRectSafe:mediaView.frame],
			  renderedNativeAd.mediaContent.hasVideoContent ? @"YES" : @"NO",
			  renderedNativeAd.mediaContent.duration,
			  renderedNativeAd.mediaContent.currentTime,
			  renderedNativeAd.mediaContent.aspectRatio);

		// 3. Bind mediaContent khi mediaView đã có window/frame thật.
		mediaView.mediaContent = renderedNativeAd.mediaContent;
		
		[self fsnForceAspectFitForSubviews:mediaView];

		dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(0.2 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
			[self fsnForceAspectFitForSubviews:mediaView];
		});

		// 4. Set video delegate sau khi bind mediaContent.
		if (renderedNativeAd.mediaContent.hasVideoContent) {
			GADVideoController *videoController = renderedNativeAd.mediaContent.videoController;
			videoController.delegate = self;

			objc_setAssociatedObject(
				videoController,
				FsnVideoControllerRequestIdKey,
				@(requestId),
				OBJC_ASSOCIATION_RETAIN_NONATOMIC
			);
		}
		
		adView.nativeAd = renderedNativeAd;

		NSLog(@"[FsnAdIOS][Show] ADDED_TO_WINDOW_AND_BOUND RequestId: %d | LayoutType=%d(%@) | MediaFrame=%@ | HasVideo=%@ | Duration=%.3f | CurrentTime=%.3f | Aspect=%.4f | CustomControls=%@ | ClickToExpand=%@",
			  requestId,
			  layoutType,
			  [self layoutNameForType:layoutType],
			  [self stringFromCGRectSafe:mediaView.frame],
			  renderedNativeAd.mediaContent.hasVideoContent ? @"YES" : @"NO",
			  renderedNativeAd.mediaContent.duration,
			  renderedNativeAd.mediaContent.currentTime,
			  renderedNativeAd.mediaContent.aspectRatio,
			  renderedNativeAd.mediaContent.videoController.customControlsEnabled ? @"YES" : @"NO",
			  renderedNativeAd.mediaContent.videoController.clickToExpandEnabled ? @"YES" : @"NO");

		dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(0.8 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
			if (!state.adContainerView || state.nativeAd != renderedNativeAd || mediaView.window == nil) {
				return;
			}

			GADVideoController *vc = renderedNativeAd.mediaContent.videoController;

			NSLog(@"[FsnAdIOS][VideoManual] RequestId=%d | customControls=%@ | hasVideo=%@ | duration=%.3f | currentTime=%.3f | aspect=%.4f",
				  requestId,
				  vc.customControlsEnabled ? @"YES" : @"NO",
				  renderedNativeAd.mediaContent.hasVideoContent ? @"YES" : @"NO",
				  renderedNativeAd.mediaContent.duration,
				  renderedNativeAd.mediaContent.currentTime,
				  renderedNativeAd.mediaContent.aspectRatio);

			if (vc.customControlsEnabled) {
				[vc play];
				NSLog(@"[FsnAdIOS][VideoManual] play() called. RequestId=%d", requestId);
			}
		});

		dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(3.0 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
			NSLog(@"[FsnAdIOS][VideoProbe] +3s RequestId=%d | InWindow=%@ | HasVideo=%@ | Duration=%.3f | CurrentTime=%.3f | Aspect=%.4f",
				  requestId,
				  mediaView.window ? @"YES" : @"NO",
				  renderedNativeAd.mediaContent.hasVideoContent ? @"YES" : @"NO",
				  renderedNativeAd.mediaContent.duration,
				  renderedNativeAd.mediaContent.currentTime,
				  renderedNativeAd.mediaContent.aspectRatio);
		});

        [self startOverlayFlowForRequestId:requestId
                             state:state
                            adView:adView
                       closeButton:closeButton
                    countDownLabel:countDownLabel
                   openStoreButton:openStoreButton
                   fakeCloseButton:fakeCloseButton
                      openStorePos:openStorePos
                          closePos:closePos
                      parentBounds:adView.bounds];
    });
}

- (void)closeButtonTapped:(UIButton*)sender {
    dispatch_async(dispatch_get_main_queue(), ^{
        NSNumber *requestIdNum = objc_getAssociatedObject(sender, (const void *)@"FsnCloseActionRequestId");
        int requestId = requestIdNum ? [requestIdNum intValue] : 0;

        FsnAdStateIOS *state = [self stateForRequestId:requestId];

        NSLog(@"[FsnAdIOS][CloseButton] Tapped. RequestId=%d | state=%@ | container=%@",
              requestId,
              state,
              state ? state.adContainerView : nil);

        // Case bình thường: chỉ dismiss đúng requestId đang hiển thị.
        if (state && state.adContainerView) {
            state.shouldCloseAfterAdClick = NO;
            [self dismissNativeAdContainer:state errorMessage:@""];
            return;
        }

        // Case lệch state: chỉ remove view orphan trên màn hình.
        // Tuyệt đối không gọi fsnForceRemoveAllFsnNativeViewsFromRoot(),
        // vì nó sẽ xoá cả state của ad gối đầu đã load xong.
        NSLog(@"[FsnAdIOS][CloseButton] State/container not found. Remove orphan views only. RequestId=%d",
              requestId);

        [self fsnForceRemoveAllFsnNativeViewsFromRootOnly];

        FsnAdStateIOS *staleState = [self stateForRequestId:requestId];
        if (staleState) {
            [self fsnClearNativeState:staleState];
            [self.nativeStates removeObjectForKey:@(requestId)];
        }

        if (_unityOnAdCompleted && requestId > 0) {
            _unityOnAdCompleted(requestId, "");
        }
    });
}

- (void)dismissNativeAdContainer:(FsnAdStateIOS*)state errorMessage:(NSString*)msg {
    if (!state) {
        return;
    }

    int requestId = state.requestId;

    dispatch_async(dispatch_get_main_queue(), ^{
        UIView *container = state.adContainerView;

        NSLog(@"[FsnAdIOS][Dismiss] Begin requestId=%d | container=%@ | superview=%@",
              requestId,
              container,
              container ? container.superview : nil);

        if (state.countDownTimer) {
            [state.countDownTimer invalidate];
            state.countDownTimer = nil;
        }

        state.adContainerView = nil;

        if (state.nativeAd.mediaContent.videoController) {
            state.nativeAd.mediaContent.videoController.delegate = nil;
        }

        state.nativeAd.delegate = nil;
        state.nativeAd = nil;
        state.adLoader = nil;
        state.isLoading = NO;

        if (container) {
			[self fsnRemoveContainerView:container reason:@"dismiss-state-container"];

			// Dọn orphan view nếu SDK/native view còn sót.
			// Chỉ remove view, không clear nativeStates của ad gối đầu.
			[self fsnForceRemoveAllFsnNativeViewsFromRootOnly];
		} else {
			NSLog(@"[FsnAdIOS][Dismiss] Container already nil requestId=%d. Run safe orphan-view cleanup.",
				  requestId);

			[self fsnForceRemoveAllFsnNativeViewsFromRootOnly];
		}

        if (_unityOnAdCompleted) {
            NSString *safeMsg = msg ?: @"";
            _unityOnAdCompleted(requestId, [safeMsg UTF8String]);
        }

        [self.nativeStates removeObjectForKey:@(requestId)];
    });
}

- (void)applyClosePosition:(UIButton*)btn
                     label:(UILabel*)lbl
                  position:(int)pos
              parentBounds:(CGRect)pb {

    float btnSize = 30;
    float margin = 16;
    float x = pb.size.width - btnSize - margin;
    float y = margin;

    switch (pos) {
        case 1:
            x = margin;
            y = margin;
            break;

        case 2:
            x = margin;
            y = pb.size.height - btnSize - margin;
            break;

        case 3:
            x = pb.size.width - btnSize - margin;
            y = pb.size.height - btnSize - margin;
            break;

        default:
            x = pb.size.width - btnSize - margin;
            y = margin;
            break;
    }

    btn.frame = CGRectMake(x, y, btnSize, btnSize);
    lbl.frame = CGRectMake(x, y, btnSize + 10, btnSize);

    // Overlay close/countdown phải giống safeRootView bên Android:
    // luôn nằm trên media, card, CTA, label.
    if (lbl.superview) {
        [lbl.superview bringSubviewToFront:lbl];
    }

    if (btn.superview) {
        [btn.superview bringSubviewToFront:btn];
    }
}

- (void)applyOverlayPosition:(UIView *)view
                    position:(int)pos
                parentBounds:(CGRect)pb
                        size:(CGSize)size
                  fakeOffset:(BOOL)fakeOffset {

    if (!view) {
        return;
    }

    CGFloat margin = 16.0;
    CGFloat offset = fakeOffset ? 30.0 : 0.0;

    CGFloat x = pb.size.width - size.width - margin;
    CGFloat y = margin;

    switch (pos) {
        case 1: // TOP_LEFT
            x = margin;
            y = margin + offset;
            break;

        case 2: // BOTTOM_LEFT
            x = margin;
            y = pb.size.height - size.height - margin - offset;
            break;

        case 3: // BOTTOM_RIGHT
            x = pb.size.width - size.width - margin;
            y = pb.size.height - size.height - margin - offset;
            break;

        case 0: // TOP_RIGHT
        default:
            x = pb.size.width - size.width - margin;
            y = margin + offset;
            break;
    }

    view.frame = CGRectMake(x, y, size.width, size.height);

    if (view.superview) {
        [view.superview bringSubviewToFront:view];
    }
}

- (void)hideAllOverlayViewsWithCloseButton:(UIButton *)closeButton
                            countDownLabel:(UILabel *)countDownLabel
                           openStoreButton:(UIView *)openStoreButton
                           fakeCloseButton:(UIView *)fakeCloseButton {

    closeButton.hidden = YES;
    countDownLabel.hidden = YES;
    openStoreButton.hidden = YES;
    fakeCloseButton.hidden = YES;
}

- (BOOL)isOverlayFlowAliveForRequestId:(int)requestId
                                 state:(FsnAdStateIOS *)state
                                adView:(UIView *)adView {

    if (!state || !state.adContainerView || !adView || !adView.superview) {
        return NO;
    }

    FsnAdStateIOS *latestState = [self stateForRequestId:requestId];

    if (!latestState || latestState != state || !latestState.adContainerView) {
        return NO;
    }

    return YES;
}

- (void)showFinalCloseForRequestId:(int)requestId
                             state:(FsnAdStateIOS *)state
                            adView:(UIView *)adView
                       closeButton:(UIButton *)closeButton
                    countDownLabel:(UILabel *)countDownLabel
                   openStoreButton:(UIView *)openStoreButton
                   fakeCloseButton:(UIView *)fakeCloseButton
                          closePos:(int)closePos
                      parentBounds:(CGRect)pb {

    if (![self isOverlayFlowAliveForRequestId:requestId state:state adView:adView]) {
        return;
    }

    [self hideAllOverlayViewsWithCloseButton:closeButton
                              countDownLabel:countDownLabel
                             openStoreButton:openStoreButton
                             fakeCloseButton:fakeCloseButton];

    [self applyOverlayPosition:closeButton
                      position:closePos
                  parentBounds:pb
                          size:CGSizeMake(35.0, 35.0)
                    fakeOffset:NO];

    closeButton.hidden = NO;
	
	if ([adView isKindOfClass:[GADNativeAdView class]]) {
		GADNativeAdView *nativeAdView = (GADNativeAdView *)adView;
		UIView *realCtaView = [adView viewWithTag:FsnRealCtaButtonTag];

		if (realCtaView) {
			nativeAdView.callToActionView = realCtaView;
			realCtaView.userInteractionEnabled = NO;

			NSLog(@"[FsnAdIOS][CTAProxy] Restore real CTA as callToActionView. RequestId=%d",
				  requestId);
		}
	}

    if (closeButton.superview) {
        [closeButton.superview bringSubviewToFront:closeButton];
    }

    NSLog(@"[FsnAdIOS][Overlay] Show final close. RequestId=%d | ClosePos=%d | Frame=%@",
          requestId,
          closePos,
          NSStringFromCGRect(closeButton.frame));
}

- (void)startOpenStoreOverlayForRequestId:(int)requestId
                                    state:(FsnAdStateIOS *)state
                                   adView:(UIView *)adView
                              closeButton:(UIButton *)closeButton
                           countDownLabel:(UILabel *)countDownLabel
                          openStoreButton:(UIView *)openStoreButton
                          fakeCloseButton:(UIView *)fakeCloseButton
                             openStorePos:(int)openStorePos
                                 closePos:(int)closePos
                             parentBounds:(CGRect)pb {

    if (![self isOverlayFlowAliveForRequestId:requestId state:state adView:adView]) {
        return;
    }

    [self hideAllOverlayViewsWithCloseButton:closeButton
                              countDownLabel:countDownLabel
                             openStoreButton:openStoreButton
                             fakeCloseButton:fakeCloseButton];

    [self applyOverlayPosition:openStoreButton
                      position:openStorePos
                  parentBounds:pb
                          size:CGSizeMake(100.0, 40.0)
                    fakeOffset:NO];

    openStoreButton.hidden = NO;
	
	if ([adView isKindOfClass:[GADNativeAdView class]]) {
    GADNativeAdView *nativeAdView = (GADNativeAdView *)adView;

    // Tạm thời biến ảnh openStore thành vùng click CTA thật.
    nativeAdView.callToActionView = openStoreButton;

    // Giữ NO để Google Mobile Ads SDK tự xử lý click.
    openStoreButton.userInteractionEnabled = NO;

    NSLog(@"[FsnAdIOS][CTAProxy] OpenStore image is now callToActionView. RequestId=%d",
          requestId);
}
    if (openStoreButton.superview) {
        [openStoreButton.superview bringSubviewToFront:openStoreButton];
    }

    NSInteger showSec = MAX(1, state.countDownSec);

    NSLog(@"[FsnAdIOS][Overlay] Show openstore fake. RequestId=%d | OpenStorePos=%d | Duration=%lds | Frame=%@",
          requestId,
          openStorePos,
          (long)showSec,
          NSStringFromCGRect(openStoreButton.frame));

    __weak FsnAdIOS *weakSelf = self;

    state.countDownTimer = [NSTimer scheduledTimerWithTimeInterval:(NSTimeInterval)showSec
                                                            repeats:NO
                                                              block:^(NSTimer * _Nonnull timer) {
        FsnAdIOS *strongSelf = weakSelf;
        if (!strongSelf) {
            return;
        }

        state.countDownTimer = nil;

        [strongSelf showFinalCloseForRequestId:requestId
                                         state:state
                                        adView:adView
                                   closeButton:closeButton
                                countDownLabel:countDownLabel
                               openStoreButton:openStoreButton
                               fakeCloseButton:fakeCloseButton
                                      closePos:closePos
                                  parentBounds:pb];
    }];
}

- (void)startFakeCloseOverlayForRequestId:(int)requestId
                                    state:(FsnAdStateIOS *)state
                                   adView:(UIView *)adView
                              closeButton:(UIButton *)closeButton
                           countDownLabel:(UILabel *)countDownLabel
                          openStoreButton:(UIView *)openStoreButton
                          fakeCloseButton:(UIView *)fakeCloseButton
                             openStorePos:(int)openStorePos
                                 closePos:(int)closePos
                             parentBounds:(CGRect)pb {

    if (![self isOverlayFlowAliveForRequestId:requestId state:state adView:adView]) {
        return;
    }

    [self hideAllOverlayViewsWithCloseButton:closeButton
                              countDownLabel:countDownLabel
                             openStoreButton:openStoreButton
                             fakeCloseButton:fakeCloseButton];

    [self applyOverlayPosition:fakeCloseButton
                      position:openStorePos
                  parentBounds:pb
                          size:CGSizeMake(35.0, 35.0)
                    fakeOffset:YES];

    fakeCloseButton.hidden = NO;
	
	if ([adView isKindOfClass:[GADNativeAdView class]]) {
    GADNativeAdView *nativeAdView = (GADNativeAdView *)adView;

    // Tạm thời biến ảnh fake close thành vùng click CTA thật.
    nativeAdView.callToActionView = fakeCloseButton;

    // Giữ NO để Google Mobile Ads SDK tự xử lý click.
    fakeCloseButton.userInteractionEnabled = NO;

    NSLog(@"[FsnAdIOS][CTAProxy] FakeClose image is now callToActionView. RequestId=%d",
          requestId);
}

    if (fakeCloseButton.superview) {
        [fakeCloseButton.superview bringSubviewToFront:fakeCloseButton];
    }

    NSInteger showSec = MAX(1, state.countDownSec);

    NSLog(@"[FsnAdIOS][Overlay] Show fake close. RequestId=%d | OpenStorePos=%d | Duration=%lds | Frame=%@",
          requestId,
          openStorePos,
          (long)showSec,
          NSStringFromCGRect(fakeCloseButton.frame));

    __weak FsnAdIOS *weakSelf = self;

    state.countDownTimer = [NSTimer scheduledTimerWithTimeInterval:(NSTimeInterval)showSec
                                                            repeats:NO
                                                              block:^(NSTimer * _Nonnull timer) {
        FsnAdIOS *strongSelf = weakSelf;
        if (!strongSelf) {
            return;
        }

        state.countDownTimer = nil;

        [strongSelf showFinalCloseForRequestId:requestId
                                         state:state
                                        adView:adView
                                   closeButton:closeButton
                                countDownLabel:countDownLabel
                               openStoreButton:openStoreButton
                               fakeCloseButton:fakeCloseButton
                                      closePos:closePos
                                  parentBounds:pb];
    }];
}

- (void)startNumberCountDownOverlayForRequestId:(int)requestId
                                          state:(FsnAdStateIOS *)state
                                         adView:(UIView *)adView
                                    closeButton:(UIButton *)closeButton
                                 countDownLabel:(UILabel *)countDownLabel
                                openStoreButton:(UIView *)openStoreButton
                                fakeCloseButton:(UIView *)fakeCloseButton
                                   openStorePos:(int)openStorePos
                                       closePos:(int)closePos
                                   parentBounds:(CGRect)pb {

    if (![self isOverlayFlowAliveForRequestId:requestId state:state adView:adView]) {
        return;
    }

    if (state.currentRemainingSec <= 0) {
        [self showFinalCloseForRequestId:requestId
                                   state:state
                                  adView:adView
                             closeButton:closeButton
                          countDownLabel:countDownLabel
                         openStoreButton:openStoreButton
                         fakeCloseButton:fakeCloseButton
                                closePos:closePos
                            parentBounds:pb];
        return;
    }

    [self hideAllOverlayViewsWithCloseButton:closeButton
                              countDownLabel:countDownLabel
                             openStoreButton:openStoreButton
                             fakeCloseButton:fakeCloseButton];

    [self applyOverlayPosition:countDownLabel
                      position:openStorePos
                  parentBounds:pb
                          size:CGSizeMake(35.0, 35.0)
                    fakeOffset:NO];

    countDownLabel.layer.cornerRadius = 17.5;
    countDownLabel.text = [NSString stringWithFormat:@"%ld", (long)state.currentRemainingSec];
    countDownLabel.hidden = NO;

    if (countDownLabel.superview) {
        [countDownLabel.superview bringSubviewToFront:countDownLabel];
    }

    NSLog(@"[FsnAdIOS][Overlay] Countdown tick. RequestId=%d | Remain=%lds | Frame=%@",
          requestId,
          (long)state.currentRemainingSec,
          NSStringFromCGRect(countDownLabel.frame));

    __weak FsnAdIOS *weakSelf = self;

    state.countDownTimer = [NSTimer scheduledTimerWithTimeInterval:1.0
                                                            repeats:NO
                                                              block:^(NSTimer * _Nonnull timer) {
        FsnAdIOS *strongSelf = weakSelf;
        if (!strongSelf) {
            return;
        }

        state.countDownTimer = nil;
        state.currentRemainingSec--;

        [strongSelf startNumberCountDownOverlayForRequestId:requestId
                                                      state:state
                                                     adView:adView
                                                closeButton:closeButton
                                             countDownLabel:countDownLabel
                                            openStoreButton:openStoreButton
                                            fakeCloseButton:fakeCloseButton
                                               openStorePos:openStorePos
                                                   closePos:closePos
                                               parentBounds:pb];
    }];
}

- (void)startOverlayFlowForRequestId:(int)requestId
                               state:(FsnAdStateIOS *)state
                              adView:(UIView *)adView
                         closeButton:(UIButton *)closeButton
                      countDownLabel:(UILabel *)countDownLabel
                     openStoreButton:(UIView *)openStoreButton
                     fakeCloseButton:(UIView *)fakeCloseButton
                        openStorePos:(int)openStorePos
                            closePos:(int)closePos
                        parentBounds:(CGRect)pb {

    if (state.countDownTimer) {
        [state.countDownTimer invalidate];
        state.countDownTimer = nil;
    }

    state.currentRemainingSec = MAX(1, state.countDownSec);

    [self hideAllOverlayViewsWithCloseButton:closeButton
                              countDownLabel:countDownLabel
                             openStoreButton:openStoreButton
                             fakeCloseButton:fakeCloseButton];

    NSTimeInterval delay = MAX(0, state.delayForCountDown);

    NSLog(@"[FsnAdIOS][Overlay] Start flow. RequestId=%d | NativeType=%d(%@) | Delay=%.2fs | CountDown=%lds | OpenStorePos=%d | ClosePos=%d",
          requestId,
          state.nativeType,
          [self nativeTypeNameForType:state.nativeType],
          delay,
          (long)state.currentRemainingSec,
          openStorePos,
          closePos);

    __weak FsnAdIOS *weakSelf = self;

    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(delay * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
        FsnAdIOS *strongSelf = weakSelf;
        if (!strongSelf) {
            return;
        }

        if (![strongSelf isOverlayFlowAliveForRequestId:requestId state:state adView:adView]) {
            return;
        }

        switch (state.nativeType) {
            case FsnAdTypeNormal: {
                [strongSelf startOpenStoreOverlayForRequestId:requestId
                                                        state:state
                                                       adView:adView
                                                  closeButton:closeButton
                                               countDownLabel:countDownLabel
                                              openStoreButton:openStoreButton
                                              fakeCloseButton:fakeCloseButton
                                                 openStorePos:openStorePos
                                                     closePos:closePos
                                                 parentBounds:pb];
                break;
			}

            case FsnAdTypeFakeClose: {
                [strongSelf startFakeCloseOverlayForRequestId:requestId
                                                        state:state
                                                       adView:adView
                                                  closeButton:closeButton
                                               countDownLabel:countDownLabel
                                              openStoreButton:openStoreButton
                                              fakeCloseButton:fakeCloseButton
                                                 openStorePos:openStorePos
                                                     closePos:closePos
                                                 parentBounds:pb];
                break;
			}

            case FsnAdTypeCountdown: {
                [strongSelf startNumberCountDownOverlayForRequestId:requestId
                                                              state:state
                                                             adView:adView
                                                        closeButton:closeButton
                                                     countDownLabel:countDownLabel
                                                    openStoreButton:openStoreButton
                                                    fakeCloseButton:fakeCloseButton
                                                       openStorePos:openStorePos
                                                           closePos:closePos
                                                       parentBounds:pb];
                break;
			}
			
            case FsnAdTypeCollapsible: {
				// Collapsible giống Android layout_cn_dialog:
				// không hiện openstore/fake/countdown, chỉ hiện nút close trong panel trên sau delay + countDownSec.
				state.countDownTimer = [NSTimer scheduledTimerWithTimeInterval:(NSTimeInterval)MAX(1, state.countDownSec)
																		repeats:NO
																		  block:^(NSTimer * _Nonnull timer) {
					state.countDownTimer = nil;

					if (![strongSelf isOverlayFlowAliveForRequestId:requestId state:state adView:adView]) {
						return;
					}

					[strongSelf hideAllOverlayViewsWithCloseButton:closeButton
													countDownLabel:countDownLabel
												   openStoreButton:openStoreButton
												   fakeCloseButton:fakeCloseButton];

					closeButton.hidden = NO;

					if (closeButton.superview) {
						[closeButton.superview bringSubviewToFront:closeButton];
					}

					NSLog(@"[FsnAdIOS][Overlay] Show collapsible close. RequestId=%d | Frame=%@",
						  requestId,
						  NSStringFromCGRect(closeButton.frame));
				}];
				break;
			}

            default: {
                [strongSelf showFinalCloseForRequestId:requestId
                                                 state:state
                                                adView:adView
                                           closeButton:closeButton
                                        countDownLabel:countDownLabel
                                       openStoreButton:openStoreButton
                                       fakeCloseButton:fakeCloseButton
                                              closePos:closePos
                                          parentBounds:pb];
                break;
			}
        }
    });
}

// MARK: - GADNativeAdDelegate Implementation

- (FsnAdStateIOS *)stateForNativeAd:(GADNativeAd *)nativeAd {
    if (!nativeAd) {
        return nil;
    }

    for (NSNumber *key in self.nativeStates) {
        FsnAdStateIOS *state = self.nativeStates[key];
        if (state.nativeAd == nativeAd) {
            return state;
        }
    }

    return nil;
}

- (void)fsnScheduleNativeClickActiveFailSafeForRequestId:(int)requestId
                                                nativeAd:(GADNativeAd *)nativeAd {

    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(1.2 * NSEC_PER_SEC)),
                   dispatch_get_main_queue(), ^{

        FsnAdStateIOS *latestState = [self stateForRequestId:requestId];

        if (!latestState || latestState.nativeAd != nativeAd || !latestState.adContainerView) {
            return;
        }

        if (!latestState.shouldCloseAfterAdClick) {
            return;
        }

        // Nếu click store thành công và app đã background thì để flow cũ xử lý khi app active lại.
        if (UIApplication.sharedApplication.applicationState != UIApplicationStateActive) {
            NSLog(@"[FsnAdIOS][ClickFailSafe] App is not active. Keep waiting for didBecomeActive. RequestId=%d",
                  requestId);
            return;
        }

        // Case lỗi kiểu: No ID provided for product.
        // SDK đã nhận click nhưng app vẫn active, không mở Store, user dễ bị kẹt.
        NSLog(@"[FsnAdIOS][ClickFailSafe] App still active after native click. Force close native ad. RequestId=%d",
              requestId);

        latestState.shouldCloseAfterAdClick = NO;
        [self dismissNativeAdContainer:latestState errorMessage:@""];
    });
}

- (void)nativeAdDidRecordClick:(GADNativeAd *)nativeAd {
    FsnAdStateIOS *state = [self stateForNativeAd:nativeAd];

    if (!state) {
        NSLog(@"[FsnAdIOS][Click] nativeAdDidRecordClick ignored. State not found.");
        return;
    }

    int requestId = state.requestId;

    if (state.nativeType == FsnAdTypeCollapsible) {
        NSLog(@"[FsnAdIOS][Click] Collapsible clicked. Close native container immediately. RequestId=%d",
              requestId);

        state.shouldCloseAfterAdClick = NO;

        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(0.05 * NSEC_PER_SEC)),
                       dispatch_get_main_queue(), ^{
            FsnAdStateIOS *latestState = [self stateForRequestId:requestId];

            if (!latestState || latestState.nativeAd != nativeAd || !latestState.adContainerView) {
                NSLog(@"[FsnAdIOS][Click] Collapsible immediate close ignored. State already gone. RequestId=%d",
                      requestId);
                return;
            }

            [self dismissNativeAdContainer:latestState errorMessage:@""];
        });

        return;
    }

    state.shouldCloseAfterAdClick = YES;

    NSLog(@"[FsnAdIOS][Click] nativeAdDidRecordClick RequestId=%d. Wait return/app dismiss, with active fail-safe.",
          state.requestId);

    [self fsnScheduleNativeClickActiveFailSafeForRequestId:requestId
                                                  nativeAd:nativeAd];
}

- (void)nativeAdWillPresentScreen:(GADNativeAd *)nativeAd {
    FsnAdStateIOS *state = [self stateForNativeAd:nativeAd];

    NSLog(@"[FsnAdIOS][Click] nativeAdWillPresentScreen RequestId: %d",
          state ? state.requestId : 0);

    if (!state) {
        return;
    }

    // Phòng trường hợp willPresentScreen xảy ra nhưng didRecordClick không route đúng.
    state.shouldCloseAfterAdClick = YES;

    [self fsnScheduleNativeClickActiveFailSafeForRequestId:state.requestId
                                                  nativeAd:nativeAd];
}

- (void)nativeAdDidDismissScreen:(GADNativeAd *)nativeAd {
    FsnAdStateIOS *state = [self stateForNativeAd:nativeAd];

    NSLog(@"[FsnAdIOS][Click] nativeAdDidDismissScreen RequestId: %d",
          state ? state.requestId : 0);

    if (!state || !state.shouldCloseAfterAdClick || !state.adContainerView) {
        return;
    }

    state.shouldCloseAfterAdClick = NO;

    NSLog(@"[FsnAdIOS][Click] Native ad screen dismissed. Close native container. RequestId=%d",
          state.requestId);

    [self dismissNativeAdContainer:state errorMessage:@""];
}

// MARK: - GADVideoControllerDelegate Implementation

- (int)requestIdForVideoController:(GADVideoController *)videoController {
    NSNumber *requestIdNum = objc_getAssociatedObject(videoController, FsnVideoControllerRequestIdKey);
    return requestIdNum ? [requestIdNum intValue] : 0;
}

- (void)videoControllerDidPlayVideo:(GADVideoController *)videoController {
    NSLog(@"[FsnAdIOS][Video] DidPlay RequestId: %d", [self requestIdForVideoController:videoController]);
}

- (void)videoControllerDidPauseVideo:(GADVideoController *)videoController {
    NSLog(@"[FsnAdIOS][Video] DidPause RequestId: %d", [self requestIdForVideoController:videoController]);
}

- (void)videoControllerDidEndVideoPlayback:(GADVideoController *)videoController {
    NSLog(@"[FsnAdIOS][Video] DidEndPlayback RequestId: %d", [self requestIdForVideoController:videoController]);
}

- (void)videoControllerDidMuteVideo:(GADVideoController *)videoController {
    NSLog(@"[FsnAdIOS][Video] DidMute RequestId: %d", [self requestIdForVideoController:videoController]);
}

- (void)videoControllerDidUnmuteVideo:(GADVideoController *)videoController {
    NSLog(@"[FsnAdIOS][Video] DidUnmute RequestId: %d", [self requestIdForVideoController:videoController]);
}

// MARK: - LAYOUT COMPONENT TRANSLATIONS


// MARK: - COLLAPSIBLE LAYOUT

- (void)applyLayoutCollapsible:(UIView*)container
                        adView:(GADNativeAdView*)adView
                          icon:(UIView*)icon
                      headline:(UILabel*)head
                          body:(UILabel*)body
                           cta:(UIView*)cta
                         media:(UIView*)media
                   closeButton:(UIButton*)closeButton
                        bounds:(CGRect)b {

    // Android layout_cn_dialog root: match width, height=350dp, background=#000.
    container.backgroundColor = [UIColor clearColor];

    adView.frame = b;
    adView.backgroundColor = [UIColor blackColor];
    adView.clipsToBounds = YES;

    icon.hidden = NO;
    head.hidden = NO;
    body.hidden = NO;
    cta.hidden = NO;
    media.hidden = NO;
    closeButton.hidden = YES;

    CGFloat screenW = b.size.width;
    CGFloat screenH = b.size.height;

    CGFloat topPanelH = MIN(150.0, screenH);
    CGFloat mediaH = MAX(0.0, screenH - topPanelH);

    UIView *topPanel = [[UIView alloc] initWithFrame:CGRectMake(0.0, 0.0, screenW, topPanelH)];
    topPanel.backgroundColor = [UIColor colorWithRed:244.0/255.0
                                               green:244.0/255.0
                                                blue:244.0/255.0
                                               alpha:1.0];
    topPanel.userInteractionEnabled = NO;
    topPanel.clipsToBounds = YES;
    [adView insertSubview:topPanel atIndex:0];

    CGFloat paddingX = 16.0;
    CGFloat paddingTop = 8.0;

    CGFloat iconSize = 60.0;
    icon.frame = CGRectMake(paddingX, paddingTop, iconSize, iconSize);

    if ([icon isKindOfClass:[UIImageView class]]) {
        UIImageView *iconImage = (UIImageView *)icon;
        iconImage.contentMode = UIViewContentModeScaleAspectFit;
        iconImage.clipsToBounds = YES;
    }

    CGFloat titleX = CGRectGetMaxX(icon.frame) + 8.0;
    CGFloat titleY = paddingTop;
    CGFloat closeReserveW = 44.0;
    CGFloat titleW = screenW - titleX - paddingX - closeReserveW;
    CGFloat titleH = 60.0;

    titleW = MAX(40.0, titleW);

    head.frame = CGRectMake(titleX, titleY + 4.0, titleW, 28.0);
    head.textColor = [UIColor colorWithRed:34.0/255.0
                                     green:34.0/255.0
                                      blue:34.0/255.0
                                     alpha:1.0];
    head.font = [UIFont boldSystemFontOfSize:20.0];
    head.textAlignment = NSTextAlignmentLeft;
    head.numberOfLines = 1;
    head.lineBreakMode = NSLineBreakByTruncatingTail;

    UILabel *adLabel = [adView viewWithTag:FsnAdLabelTag];
    if (!adLabel || ![adLabel isKindOfClass:[UILabel class]]) {
        adLabel = [[UILabel alloc] init];
        adLabel.tag = FsnAdLabelTag;
        adLabel.userInteractionEnabled = NO;
        adLabel.text = @"Ad";
        adLabel.textColor = [UIColor whiteColor];
        adLabel.font = [UIFont boldSystemFontOfSize:11.0];
        adLabel.textAlignment = NSTextAlignmentCenter;
        adLabel.backgroundColor = [[UIColor blackColor] colorWithAlphaComponent:0.55];
        adLabel.layer.cornerRadius = 4.0;
        adLabel.clipsToBounds = YES;
        [adView addSubview:adLabel];
    }

    CGFloat metaY = titleY + 34.0;
    adLabel.frame = CGRectMake(titleX, metaY, 28.0, 18.0);

    UILabel *subHeadline = [adView viewWithTag:FsnCollapsibleSubHeadlineTag];
    if (!subHeadline || ![subHeadline isKindOfClass:[UILabel class]]) {
        subHeadline = [[UILabel alloc] init];
        subHeadline.tag = FsnCollapsibleSubHeadlineTag;
        subHeadline.userInteractionEnabled = NO;
        subHeadline.text = @"Sponsored app \u00B7 4.8 \u2605";
        subHeadline.textColor = [UIColor colorWithRed:102.0/255.0
                                                green:102.0/255.0
                                                 blue:102.0/255.0
                                                alpha:1.0];
        subHeadline.font = [UIFont systemFontOfSize:13.0];
        subHeadline.textAlignment = NSTextAlignmentLeft;
        subHeadline.numberOfLines = 1;
        subHeadline.lineBreakMode = NSLineBreakByTruncatingTail;
        [adView addSubview:subHeadline];
    }

    CGFloat subX = CGRectGetMaxX(adLabel.frame) + 6.0;
    subHeadline.frame = CGRectMake(subX, metaY, MAX(10.0, titleX + titleW - subX), 18.0);

    CGFloat bodyY = paddingTop + titleH + 4.0;
    body.frame = CGRectMake(paddingX, bodyY, screenW - paddingX * 2.0, 40.0);
    body.textColor = [UIColor colorWithRed:51.0/255.0
                                     green:51.0/255.0
                                      blue:51.0/255.0
                                     alpha:1.0];
    body.font = [UIFont systemFontOfSize:16.0];
    body.textAlignment = NSTextAlignmentLeft;
    body.numberOfLines = 2;
    body.lineBreakMode = NSLineBreakByTruncatingTail;

    CGFloat ctaY = CGRectGetMaxY(body.frame) + 8.0;
    CGFloat ctaH = 25.0;
    if (ctaY + ctaH > topPanelH) {
        ctaY = MAX(0.0, topPanelH - ctaH - 5.0);
    }

    cta.frame = CGRectMake(paddingX, ctaY, screenW - paddingX * 2.0, ctaH);

    if ([cta isKindOfClass:[UIButton class]]) {
        UIButton *button = (UIButton *)cta;
        button.titleLabel.font = [UIFont boldSystemFontOfSize:19.0];
        button.titleLabel.textAlignment = NSTextAlignmentCenter;
        button.layer.cornerRadius = 6.0;
        button.clipsToBounds = YES;
        [button setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    }

    cta.backgroundColor = [UIColor colorWithRed:0.0
                                          green:0.45
                                           blue:1.0
                                          alpha:1.0];

    media.frame = CGRectMake(0.0, topPanelH, screenW, mediaH);
    media.backgroundColor = [UIColor blackColor];
    media.clipsToBounds = YES;
    media.layer.masksToBounds = YES;
    media.contentMode = UIViewContentModeScaleAspectFit;

    // Android btnClose: 36dp top|end inside top panel, image padding around 5dp.
    CGFloat closeSize = 36.0;
    CGFloat closeX = screenW - paddingX - closeSize;
    CGFloat closeY = paddingTop;
    closeButton.frame = CGRectMake(closeX, closeY, closeSize, closeSize);
    closeButton.contentEdgeInsets = UIEdgeInsetsMake(7.0, 7.0, 7.0, 7.0);
    closeButton.backgroundColor = [UIColor clearColor];
    closeButton.layer.cornerRadius = 18.0;
    closeButton.clipsToBounds = YES;
    closeButton.hidden = YES;

    [adView bringSubviewToFront:topPanel];
    [adView bringSubviewToFront:icon];
    [adView bringSubviewToFront:head];
    [adView bringSubviewToFront:adLabel];
    [adView bringSubviewToFront:subHeadline];
    [adView bringSubviewToFront:body];
    [adView bringSubviewToFront:cta];
    [adView bringSubviewToFront:media];
    if (closeButton.superview) {
        [closeButton.superview bringSubviewToFront:closeButton];
    }

    NSLog(@"[FsnAdIOS][LayoutCollapsibleAndroidLike] bounds=%@ | top=%@ | icon=%@ | head=%@ | adLabel=%@ | sub=%@ | body=%@ | cta=%@ | media=%@ | close=%@",
          NSStringFromCGRect(b),
          NSStringFromCGRect(topPanel.frame),
          NSStringFromCGRect(icon.frame),
          NSStringFromCGRect(head.frame),
          NSStringFromCGRect(adLabel.frame),
          NSStringFromCGRect(subHeadline.frame),
          NSStringFromCGRect(body.frame),
          NSStringFromCGRect(cta.frame),
          NSStringFromCGRect(media.frame),
          NSStringFromCGRect(closeButton.frame));
}

- (void)applyLayoutStyle1:(UIView*)container
                   adView:(GADNativeAdView*)adView
                     icon:(UIView*)icon
                 headline:(UILabel*)head
                     body:(UILabel*)body
                      cta:(UIView*)cta
                    media:(UIView*)media
                   bounds:(CGRect)b {

    // Android XML root background: #7D7D7D
    container.backgroundColor = [UIColor colorWithRed:125.0/255.0
                                                green:125.0/255.0
                                                 blue:125.0/255.0
                                                alpha:1.0];

    adView.frame = container.bounds;
    adView.backgroundColor = [UIColor clearColor];
    adView.clipsToBounds = YES;

    // Reset visibility để tránh view bị hidden từ layout/style trước
    icon.hidden = NO;
    head.hidden = NO;
    body.hidden = NO;
    cta.hidden = NO;
    media.hidden = NO;

    // Text colors giống Android bottom card
    head.textColor = [UIColor colorWithRed:34.0/255.0
                                     green:34.0/255.0
                                      blue:34.0/255.0
                                     alpha:1.0];

    body.textColor = [UIColor colorWithRed:51.0/255.0
                                     green:51.0/255.0
                                      blue:51.0/255.0
                                     alpha:1.0];

    head.font = [UIFont boldSystemFontOfSize:20.0];
    body.font = [UIFont systemFontOfSize:16.0];

    head.numberOfLines = 2;
    body.numberOfLines = 3;

    // CTA giống Android: cao 32dp, full width trong card
    if ([cta isKindOfClass:[UIButton class]]) {
        UIButton *button = (UIButton *)cta;
        button.titleLabel.font = [UIFont boldSystemFontOfSize:19.0];
        button.layer.cornerRadius = 6.0;
        button.clipsToBounds = YES;
    }

    cta.backgroundColor = [UIColor colorWithRed:0.0
                                          green:0.45
                                           blue:1.0
                                          alpha:1.0];

    // ===== Android-like layout metrics =====
    CGFloat screenW = b.size.width;
    CGFloat screenH = b.size.height;

    CGFloat horizontalPadding = 16.0;
    CGFloat topSafeAreaH = 72.0;

    CGFloat mediaH = 200.0;
    CGFloat mediaTopPadding = 16.0;

    CGFloat infoTopPadding = 16.0;
    CGFloat iconSize = 40.0;
    CGFloat iconToTextGap = 12.0;

    CGFloat headlineH = 48.0;
    CGFloat bodyTopGap = 8.0;
    CGFloat bodyH = 66.0;

    CGFloat infoContentH = MAX(iconSize, headlineH + bodyTopGap + bodyH);

    CGFloat ctaTopGap = 16.0;
    CGFloat ctaH = 32.0;
    CGFloat ctaBottomGap = 16.0;

    CGFloat bottomCardH =
        mediaTopPadding +
        mediaH +
        infoTopPadding +
        infoContentH +
        ctaTopGap +
        ctaH +
        ctaBottomGap;

    // Nếu màn quá thấp thì co media lại, tránh card vượt màn
    CGFloat maxAllowedCardH = screenH - topSafeAreaH;
    if (bottomCardH > maxAllowedCardH) {
        CGFloat overflow = bottomCardH - maxAllowedCardH;
        mediaH = MAX(120.0, mediaH - overflow);
        bottomCardH =
            mediaTopPadding +
            mediaH +
            infoTopPadding +
            infoContentH +
            ctaTopGap +
            ctaH +
            ctaBottomGap;
    }

    CGFloat bottomCardX = 0.0;
    CGFloat bottomCardY = screenH - bottomCardH;
    CGFloat bottomCardW = screenW;

    // Background của bottom_ad_container
    UIView *bottomBg = [[UIView alloc] initWithFrame:CGRectMake(bottomCardX,
                                                               bottomCardY,
                                                               bottomCardW,
                                                               bottomCardH)];
    bottomBg.backgroundColor = [UIColor whiteColor];
    bottomBg.userInteractionEnabled = NO;
    bottomBg.clipsToBounds = YES;

    // Bo góc trên giống fsn_bottom_ad_bg
    if (@available(iOS 11.0, *)) {
        bottomBg.layer.cornerRadius = 18.0;
        bottomBg.layer.maskedCorners = kCALayerMinXMinYCorner | kCALayerMaxXMinYCorner;
    } else {
        bottomBg.layer.cornerRadius = 18.0;
    }

    [adView insertSubview:bottomBg atIndex:0];

    // ===== MediaView: nằm gọn trong bottom card, không nằm trên màn hình =====
    CGFloat mediaX = horizontalPadding;
    CGFloat mediaY = bottomCardY + mediaTopPadding;
    CGFloat mediaW = bottomCardW - horizontalPadding * 2.0;

    media.frame = CGRectMake(mediaX, mediaY, mediaW, mediaH);
    media.backgroundColor = [UIColor clearColor];
    media.clipsToBounds = YES;
	media.layer.masksToBounds = YES;
    media.contentMode = UIViewContentModeScaleAspectFit;

    // ===== Info row =====
    CGFloat infoY = CGRectGetMaxY(media.frame) + infoTopPadding;

    icon.frame = CGRectMake(horizontalPadding,
                            infoY,
                            iconSize,
                            iconSize);

    if ([icon isKindOfClass:[UIImageView class]]) {
        UIImageView *iconImage = (UIImageView *)icon;
        iconImage.contentMode = UIViewContentModeScaleAspectFit;
        iconImage.clipsToBounds = YES;
    }

    CGFloat textX = CGRectGetMaxX(icon.frame) + iconToTextGap;
    CGFloat textW = screenW - textX - horizontalPadding;

    head.frame = CGRectMake(textX,
                            infoY,
                            textW,
                            headlineH);

    body.frame = CGRectMake(textX,
                            CGRectGetMaxY(head.frame) + bodyTopGap,
                            textW,
                            bodyH);

    // ===== CTA =====
    CGFloat ctaY = infoY + infoContentH + ctaTopGap;

    cta.frame = CGRectMake(horizontalPadding,
                           ctaY,
                           screenW - horizontalPadding * 2.0,
                           ctaH);
						   
	// Ad label top-left giống các template khác
	UILabel *adLabel = [adView viewWithTag:FsnAdLabelTag];
	if (!adLabel || ![adLabel isKindOfClass:[UILabel class]]) {
		adLabel = [[UILabel alloc] init];
		adLabel.tag = FsnAdLabelTag;
		adLabel.userInteractionEnabled = NO;
		adLabel.text = @"Ad";
		adLabel.textColor = [UIColor whiteColor];
		adLabel.font = [UIFont boldSystemFontOfSize:11.0];
		adLabel.textAlignment = NSTextAlignmentCenter;
		adLabel.backgroundColor = [[UIColor blackColor] colorWithAlphaComponent:0.55];
		adLabel.layer.cornerRadius = 4.0;
		adLabel.clipsToBounds = YES;
		[adView addSubview:adLabel];
	}

	adLabel.frame = CGRectMake(12.0, 10.0, 28.0, 18.0);

    // Đảm bảo asset nằm trên background
    [adView bringSubviewToFront:media];
	[adView bringSubviewToFront:icon];
	[adView bringSubviewToFront:head];
	[adView bringSubviewToFront:body];
	[adView bringSubviewToFront:cta];
	[adView bringSubviewToFront:adLabel];

    NSLog(@"[FsnAdIOS][LayoutStyle1AndroidLike_FIXED] bounds=%@ | bottomCard=%@ | media=%@ | icon=%@ | head=%@ | body=%@ | cta=%@",
          NSStringFromCGRect(b),
          NSStringFromCGRect(bottomBg.frame),
          NSStringFromCGRect(media.frame),
          NSStringFromCGRect(icon.frame),
          NSStringFromCGRect(head.frame),
          NSStringFromCGRect(body.frame),
          NSStringFromCGRect(cta.frame));
}

- (void)applyLayoutStyle2:(UIView*)container
                   adView:(GADNativeAdView*)adView
                     icon:(UIView*)icon
                 headline:(UILabel*)head
                     body:(UILabel*)body
                      cta:(UIView*)cta
                    media:(UIView*)media
                   bounds:(CGRect)b {

    // Android root background: #1E1E1E
    container.backgroundColor = [UIColor colorWithRed:30.0/255.0
                                                green:30.0/255.0
                                                 blue:30.0/255.0
                                                alpha:1.0];

    adView.frame = container.bounds;
    adView.backgroundColor = [UIColor clearColor];
    adView.clipsToBounds = YES;

    // Reset visibility
    media.hidden = NO;
    icon.hidden = NO;
    head.hidden = NO;
    body.hidden = NO;
    cta.hidden = NO;

    CGFloat screenW = b.size.width;
    CGFloat screenH = b.size.height;

    // Android style 2:
    // MediaView: match_parent width, height = 250dp, gravity top.
    CGFloat mediaH = 250.0;
    CGFloat mediaX = 0.0;
    CGFloat mediaY = 0.0;
    CGFloat mediaW = screenW;

    // Nếu máy thấp quá thì co nhẹ media để tránh CTA bị văng ra ngoài.
    CGFloat minBottomReserve = 16.0 + 60.0 + 12.0 + 52.0 + 8.0 + 66.0 + 16.0 + 52.0 + 8.0;
    if (mediaH + minBottomReserve > screenH) {
        mediaH = MAX(160.0, screenH - minBottomReserve);
    }

    media.frame = CGRectMake(mediaX, mediaY, mediaW, mediaH);
    media.backgroundColor = [UIColor clearColor];
    media.clipsToBounds = YES;
    media.layer.masksToBounds = YES;
    media.contentMode = UIViewContentModeScaleAspectFit;

    // Container info phía dưới media, gravity center_horizontal, padding 16.
    CGFloat padding = 16.0;
    CGFloat contentY = CGRectGetMaxY(media.frame) + padding;

    // Icon: 60dp, center horizontal
    CGFloat iconSize = 60.0;
    icon.frame = CGRectMake((screenW - iconSize) * 0.5,
                            contentY,
                            iconSize,
                            iconSize);

    if ([icon isKindOfClass:[UIImageView class]]) {
        UIImageView *iconImage = (UIImageView *)icon;
        iconImage.contentMode = UIViewContentModeScaleAspectFit;
        iconImage.clipsToBounds = YES;
    }

    // Headline: match width, marginTop 12, center, maxLines 2, white, bold 20
    CGFloat headlineTop = CGRectGetMaxY(icon.frame) + 12.0;
    CGFloat headlineH = 52.0;

    head.frame = CGRectMake(padding,
                            headlineTop,
                            screenW - padding * 2.0,
                            headlineH);

    head.textColor = [UIColor whiteColor];
    head.font = [UIFont boldSystemFontOfSize:20.0];
    head.textAlignment = NSTextAlignmentCenter;
    head.numberOfLines = 2;
    head.lineBreakMode = NSLineBreakByTruncatingTail;

    // Body: marginTop 8, center, maxLines 3, #7C7C7C, size 16
    CGFloat bodyTop = CGRectGetMaxY(head.frame) + 8.0;
    CGFloat bodyH = 70.0;

    body.frame = CGRectMake(padding,
                            bodyTop,
                            screenW - padding * 2.0,
                            bodyH);

    body.textColor = [UIColor colorWithRed:124.0/255.0
                                     green:124.0/255.0
                                      blue:124.0/255.0
                                     alpha:1.0];
    body.font = [UIFont systemFontOfSize:16.0];
    body.textAlignment = NSTextAlignmentCenter;
    body.numberOfLines = 3;
    body.lineBreakMode = NSLineBreakByTruncatingTail;

    // CTA: match width, height 52dp, marginTop 16, text black
    CGFloat ctaTop = CGRectGetMaxY(body.frame) + 16.0;
    CGFloat ctaH = 52.0;

    // Nếu CTA bị vượt màn, kéo cụm content lên một chút
    CGFloat bottomOverflow = ctaTop + ctaH + 8.0 - screenH;
    if (bottomOverflow > 0.0) {
        CGFloat shiftUp = bottomOverflow + 8.0;

        icon.frame = CGRectOffset(icon.frame, 0, -shiftUp);
        head.frame = CGRectOffset(head.frame, 0, -shiftUp);
        body.frame = CGRectOffset(body.frame, 0, -shiftUp);

        ctaTop = CGRectGetMaxY(body.frame) + 16.0;
    }

    cta.frame = CGRectMake(padding,
                           ctaTop,
                           screenW - padding * 2.0,
                           ctaH);

    if ([cta isKindOfClass:[UIButton class]]) {
        UIButton *button = (UIButton *)cta;
        button.titleLabel.font = [UIFont boldSystemFontOfSize:19.0];
        button.titleLabel.textAlignment = NSTextAlignmentCenter;
        button.layer.cornerRadius = 8.0;
        button.clipsToBounds = YES;
        [button setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
    }

    // Gần giống fsn_cta_background_style_2.
    // Nếu drawable Android của bạn là màu khác, đổi RGB ở đây.
    cta.backgroundColor = [UIColor colorWithRed:1.0
                                          green:0.82
                                           blue:0.20
                                          alpha:1.0];

    // Ad label top-left giống XML
    UILabel *adLabel = [adView viewWithTag:FsnAdLabelTag];
    if (!adLabel || ![adLabel isKindOfClass:[UILabel class]]) {
        adLabel = [[UILabel alloc] init];
        adLabel.tag = FsnAdLabelTag;
        adLabel.userInteractionEnabled = NO;
        adLabel.text = @"Ad";
        adLabel.textColor = [UIColor whiteColor];
        adLabel.font = [UIFont boldSystemFontOfSize:11.0];
        adLabel.textAlignment = NSTextAlignmentCenter;
        adLabel.backgroundColor = [[UIColor blackColor] colorWithAlphaComponent:0.55];
        adLabel.layer.cornerRadius = 4.0;
        adLabel.clipsToBounds = YES;
        [adView addSubview:adLabel];
    }

    adLabel.frame = CGRectMake(12.0, 10.0, 28.0, 18.0);

    // Đảm bảo background không che asset
    [adView bringSubviewToFront:media];
    [adView bringSubviewToFront:icon];
    [adView bringSubviewToFront:head];
    [adView bringSubviewToFront:body];
    [adView bringSubviewToFront:cta];
    [adView bringSubviewToFront:adLabel];

    NSLog(@"[FsnAdIOS][LayoutStyle2AndroidLike] bounds=%@ | media=%@ | icon=%@ | head=%@ | body=%@ | cta=%@ | adLabel=%@",
          NSStringFromCGRect(b),
          NSStringFromCGRect(media.frame),
          NSStringFromCGRect(icon.frame),
          NSStringFromCGRect(head.frame),
          NSStringFromCGRect(body.frame),
          NSStringFromCGRect(cta.frame),
          NSStringFromCGRect(adLabel.frame));
}

- (void)applyLayoutStyle3:(UIView*)container
                   adView:(GADNativeAdView*)adView
                     icon:(UIView*)icon
                 headline:(UILabel*)head
                     body:(UILabel*)body
                      cta:(UIView*)cta
                    media:(UIView*)media
                   bounds:(CGRect)b {

    // Android root background: #000
    container.backgroundColor = [UIColor blackColor];

    adView.frame = container.bounds;
    adView.backgroundColor = [UIColor clearColor];
    adView.clipsToBounds = YES;

    media.hidden = NO;
    icon.hidden = NO;
    head.hidden = NO;
    body.hidden = NO;
    cta.hidden = NO;

    CGFloat screenW = b.size.width;
    CGFloat screenH = b.size.height;

    // ===== Android ad_info_container =====
    // width = match_parent
    // height = wrap_content
    // marginBottom = 8dp
    // background = #F4F4F4
    // padding = 16dp

    CGFloat infoMarginBottom = 8.0;
    CGFloat padding = 16.0;

    CGFloat iconSize = 28.0;
    CGFloat iconToHeadGap = 8.0;

    CGFloat headlineH = 48.0; // maxLines=2, textSize=20sp
    CGFloat rowH = MAX(iconSize, headlineH);

    CGFloat bodyTopMargin = 12.0;
    CGFloat bodyH = 70.0; // maxLines=3, textSize=16sp

    CGFloat ctaTopMargin = 16.0;
    CGFloat ctaH = 52.0;

    CGFloat infoH =
        padding +
        rowH +
        bodyTopMargin +
        bodyH +
        ctaTopMargin +
        ctaH +
        padding;

    // Đảm bảo media vẫn có vùng hiển thị tối thiểu nếu màn thấp.
    CGFloat minMediaH = 180.0;
    if (screenH - infoH - infoMarginBottom < minMediaH) {
        CGFloat overflow = minMediaH - (screenH - infoH - infoMarginBottom);

        bodyH = MAX(44.0, bodyH - overflow);

        infoH =
            padding +
            rowH +
            bodyTopMargin +
            bodyH +
            ctaTopMargin +
            ctaH +
            padding;
    }

    CGFloat infoX = 0.0;
    CGFloat infoY = screenH - infoMarginBottom - infoH;
    CGFloat infoW = screenW;

    UIView *infoBg = [[UIView alloc] initWithFrame:CGRectMake(infoX, infoY, infoW, infoH)];
    infoBg.backgroundColor = [UIColor colorWithRed:244.0/255.0
                                             green:244.0/255.0
                                              blue:244.0/255.0
                                             alpha:1.0];
    infoBg.userInteractionEnabled = NO;
    infoBg.clipsToBounds = YES;

    [adView insertSubview:infoBg atIndex:0];

    // ===== Android MediaView slot =====
	// Android layout_weight=1 nghĩa là vùng media chiếm toàn bộ phần trên.
	// Nhưng trên iOS không nên kéo chính GADMediaView thành khung quá dọc,
	// vì dễ làm SDK render poster/image bị stretch và video không start.
	CGFloat mediaSlotX = 0.0;
	CGFloat mediaSlotY = 0.0;
	CGFloat mediaSlotW = screenW;
	CGFloat mediaSlotH = infoY;

	UIView *mediaSlotBg = [[UIView alloc] initWithFrame:CGRectMake(mediaSlotX,
																   mediaSlotY,
																   mediaSlotW,
																   mediaSlotH)];
	mediaSlotBg.backgroundColor = [UIColor blackColor];
	mediaSlotBg.userInteractionEnabled = NO;
	mediaSlotBg.clipsToBounds = YES;

	[adView insertSubview:mediaSlotBg atIndex:0];

	// Actual GADMediaView: aspect-fit trong vùng media slot.
	// Dùng 16:9 mặc định vì aspectRatio thường vẫn = 0 trước khi video metadata sẵn sàng.
	CGFloat fallbackAspect = 16.0 / 9.0;
	CGFloat targetW = mediaSlotW;
	CGFloat targetH = targetW / fallbackAspect;

	if (targetH > mediaSlotH) {
		targetH = mediaSlotH;
		targetW = targetH * fallbackAspect;
	}

	CGFloat mediaX = (mediaSlotW - targetW) * 0.5;
	CGFloat mediaY = mediaSlotY + (mediaSlotH - targetH) * 0.5;

	media.frame = CGRectMake(mediaX, mediaY, targetW, targetH);
	media.backgroundColor = [UIColor clearColor];
	media.clipsToBounds = YES;
	media.layer.masksToBounds = YES;
	media.contentMode = UIViewContentModeScaleAspectFit;

    // ===== Row: icon + headline =====
    CGFloat rowY = infoY + padding;

    icon.frame = CGRectMake(padding,
                            rowY + (rowH - iconSize) * 0.5,
                            iconSize,
                            iconSize);

    if ([icon isKindOfClass:[UIImageView class]]) {
        UIImageView *iconImage = (UIImageView *)icon;
        iconImage.contentMode = UIViewContentModeScaleAspectFit;
        iconImage.clipsToBounds = YES;
    }

    CGFloat headX = CGRectGetMaxX(icon.frame) + iconToHeadGap;
    CGFloat headW = screenW - headX - padding;

    head.frame = CGRectMake(headX,
                            rowY,
                            headW,
                            rowH);

    head.textColor = [UIColor colorWithRed:34.0/255.0
                                     green:34.0/255.0
                                      blue:34.0/255.0
                                     alpha:1.0];
    head.font = [UIFont boldSystemFontOfSize:20.0];
    head.textAlignment = NSTextAlignmentLeft;
    head.numberOfLines = 2;
    head.lineBreakMode = NSLineBreakByTruncatingTail;

    // ===== Body =====
    CGFloat bodyY = rowY + rowH + bodyTopMargin;

    body.frame = CGRectMake(padding,
                            bodyY,
                            screenW - padding * 2.0,
                            bodyH);

    body.textColor = [UIColor colorWithRed:51.0/255.0
                                     green:51.0/255.0
                                      blue:51.0/255.0
                                     alpha:1.0];
    body.font = [UIFont systemFontOfSize:16.0];
    body.textAlignment = NSTextAlignmentLeft;
    body.numberOfLines = 3;
    body.lineBreakMode = NSLineBreakByTruncatingTail;

    // ===== CTA =====
    CGFloat ctaY = CGRectGetMaxY(body.frame) + ctaTopMargin;

    cta.frame = CGRectMake(padding,
                           ctaY,
                           screenW - padding * 2.0,
                           ctaH);

    if ([cta isKindOfClass:[UIButton class]]) {
        UIButton *button = (UIButton *)cta;
        button.titleLabel.font = [UIFont boldSystemFontOfSize:19.0];
        button.titleLabel.textAlignment = NSTextAlignmentCenter;
        button.layer.cornerRadius = 8.0;
        button.clipsToBounds = YES;
        [button setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    }

    // Gần giống @drawable/fsn_cta_background
    cta.backgroundColor = [UIColor colorWithRed:0.0
                                          green:0.45
                                           blue:1.0
                                          alpha:1.0];

    // ===== Ad label overlay top-left =====
    UILabel *adLabel = [adView viewWithTag:FsnAdLabelTag];
    if (!adLabel || ![adLabel isKindOfClass:[UILabel class]]) {
        adLabel = [[UILabel alloc] init];
        adLabel.tag = FsnAdLabelTag;
        adLabel.userInteractionEnabled = NO;
        adLabel.text = @"Ad";
        adLabel.textColor = [UIColor whiteColor];
        adLabel.font = [UIFont boldSystemFontOfSize:11.0];
        adLabel.textAlignment = NSTextAlignmentCenter;
        adLabel.backgroundColor = [[UIColor blackColor] colorWithAlphaComponent:0.55];
        adLabel.layer.cornerRadius = 4.0;
        adLabel.clipsToBounds = YES;
        [adView addSubview:adLabel];
    }

    adLabel.frame = CGRectMake(12.0, 10.0, 28.0, 18.0);

    // Z-order
    [adView bringSubviewToFront:mediaSlotBg];
	[adView bringSubviewToFront:media];

	[adView bringSubviewToFront:infoBg];
	[adView bringSubviewToFront:icon];
	[adView bringSubviewToFront:head];
	[adView bringSubviewToFront:body];
	[adView bringSubviewToFront:cta];
	[adView bringSubviewToFront:adLabel];

    NSLog(@"[FsnAdIOS][LayoutStyle3AndroidLike_FIXED] bounds=%@ | mediaSlot=%@ | media=%@ | infoBg=%@ | icon=%@ | head=%@ | body=%@ | cta=%@ | adLabel=%@",
      NSStringFromCGRect(b),
      NSStringFromCGRect(mediaSlotBg.frame),
      NSStringFromCGRect(media.frame),
      NSStringFromCGRect(infoBg.frame),
      NSStringFromCGRect(icon.frame),
      NSStringFromCGRect(head.frame),
      NSStringFromCGRect(body.frame),
      NSStringFromCGRect(cta.frame),
      NSStringFromCGRect(adLabel.frame));
}

- (void)applyLayoutStyle4:(UIView*)container
                   adView:(GADNativeAdView*)adView
                     icon:(UIView*)icon
                 headline:(UILabel*)head
                     body:(UILabel*)body
                      cta:(UIView*)cta
                    media:(UIView*)media
                   bounds:(CGRect)b {

    // Android root background: #7D7D7D
    container.backgroundColor = [UIColor colorWithRed:125.0/255.0
                                                green:125.0/255.0
                                                 blue:125.0/255.0
                                                alpha:1.0];

    adView.frame = container.bounds;
    adView.backgroundColor = [UIColor clearColor];
    adView.clipsToBounds = YES;

    // Reset visibility
    icon.hidden = NO;
    head.hidden = NO;
    media.hidden = NO;
    body.hidden = NO;
    cta.hidden = NO;

    CGFloat screenW = b.size.width;
    CGFloat screenH = b.size.height;

    // Android:
    // layout_marginStart="20dp"
    // layout_marginEnd="20dp"
    // paddingStart/Top/End/Bottom="16dp"
    CGFloat cardMarginX = 20.0;
    CGFloat padding = 16.0;

    CGFloat cardX = cardMarginX;
    CGFloat cardW = screenW - cardMarginX * 2.0;

    // ===== Asset metrics giống XML Android =====
    CGFloat iconSize = 48.0;

    CGFloat headlineTopMargin = 12.0;
    CGFloat headlineH = 52.0; // maxLines=2, textSize=20

    CGFloat mediaTopMargin = 16.0;
    CGFloat mediaH = 200.0;

    CGFloat bodyTopMargin = 12.0;
    CGFloat bodyH = 70.0; // maxLines=3, textSize=16

    CGFloat ctaTopMargin = 16.0;
    CGFloat ctaH = 32.0;

    CGFloat cardH =
        padding +
        iconSize +
        headlineTopMargin +
        headlineH +
        mediaTopMargin +
        mediaH +
        bodyTopMargin +
        bodyH +
        ctaTopMargin +
        ctaH +
        padding;

    // Nếu màn thấp quá thì co body/media lại một chút, tránh card vượt màn.
    CGFloat maxCardH = screenH - 40.0;
    if (cardH > maxCardH) {
        CGFloat overflow = cardH - maxCardH;

        CGFloat reduceMedia = MIN(overflow, 60.0);
        mediaH = MAX(140.0, mediaH - reduceMedia);
        overflow -= reduceMedia;

        if (overflow > 0.0) {
            bodyH = MAX(44.0, bodyH - overflow);
        }

        cardH =
            padding +
            iconSize +
            headlineTopMargin +
            headlineH +
            mediaTopMargin +
            mediaH +
            bodyTopMargin +
            bodyH +
            ctaTopMargin +
            ctaH +
            padding;
    }

    // Android layout_gravity="center"
    CGFloat cardY = (screenH - cardH) * 0.5;

    UIView *cardBg = [[UIView alloc] initWithFrame:CGRectMake(cardX, cardY, cardW, cardH)];
    cardBg.backgroundColor = [UIColor whiteColor];
    cardBg.userInteractionEnabled = NO;
    cardBg.clipsToBounds = YES;

    // Giống fsn_bottom_ad_bg: card trắng bo góc.
    cardBg.layer.cornerRadius = 18.0;

    [adView insertSubview:cardBg atIndex:0];

    CGFloat contentX = cardX + padding;
    CGFloat contentW = cardW - padding * 2.0;

    // ===== Icon: 48dp, center horizontal =====
    CGFloat iconY = cardY + padding;
    icon.frame = CGRectMake(cardX + (cardW - iconSize) * 0.5,
                            iconY,
                            iconSize,
                            iconSize);

    if ([icon isKindOfClass:[UIImageView class]]) {
        UIImageView *iconImage = (UIImageView *)icon;
        iconImage.contentMode = UIViewContentModeScaleAspectFit;
        iconImage.clipsToBounds = YES;
    }

    // ===== Headline: marginTop=12, center, maxLines=2 =====
    CGFloat headY = CGRectGetMaxY(icon.frame) + headlineTopMargin;

    head.frame = CGRectMake(contentX,
                            headY,
                            contentW,
                            headlineH);

    head.textColor = [UIColor colorWithRed:34.0/255.0
                                     green:34.0/255.0
                                      blue:34.0/255.0
                                     alpha:1.0];
    head.font = [UIFont boldSystemFontOfSize:20.0];
    head.textAlignment = NSTextAlignmentCenter;
    head.numberOfLines = 2;
    head.lineBreakMode = NSLineBreakByTruncatingTail;

    // ===== MediaView: match width trong card, height=200dp, marginTop=16 =====
    CGFloat mediaY = CGRectGetMaxY(head.frame) + mediaTopMargin;

    media.frame = CGRectMake(contentX,
                             mediaY,
                             contentW,
                             mediaH);

    media.backgroundColor = [UIColor clearColor];
    media.clipsToBounds = YES;
    media.layer.masksToBounds = YES;
    media.contentMode = UIViewContentModeScaleAspectFit;

    // ===== Body: marginTop=12, center, maxLines=3 =====
    CGFloat bodyY = CGRectGetMaxY(media.frame) + bodyTopMargin;

    body.frame = CGRectMake(contentX,
                            bodyY,
                            contentW,
                            bodyH);

    body.textColor = [UIColor colorWithRed:60.0/255.0
                                     green:60.0/255.0
                                      blue:60.0/255.0
                                     alpha:1.0];
    body.font = [UIFont systemFontOfSize:16.0];
    body.textAlignment = NSTextAlignmentCenter;
    body.numberOfLines = 3;
    body.lineBreakMode = NSLineBreakByTruncatingTail;

    // ===== CTA: match width, height=32dp, marginTop=16 =====
    CGFloat ctaY = CGRectGetMaxY(body.frame) + ctaTopMargin;

    cta.frame = CGRectMake(contentX,
                           ctaY,
                           contentW,
                           ctaH);

    if ([cta isKindOfClass:[UIButton class]]) {
        UIButton *button = (UIButton *)cta;
        button.titleLabel.font = [UIFont boldSystemFontOfSize:19.0];
        button.titleLabel.textAlignment = NSTextAlignmentCenter;
        button.layer.cornerRadius = 6.0;
        button.clipsToBounds = YES;
        [button setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    }

    // Gần giống @drawable/fsn_cta_background_style_4.
    // Nếu drawable Android style_4 là màu khác, đổi RGB ở đây.
    cta.backgroundColor = [UIColor colorWithRed:0.0
                                          green:0.45
                                           blue:1.0
                                          alpha:1.0];

    // ===== Ad label overlay top-left giống safeRootView =====
    UILabel *adLabel = [adView viewWithTag:FsnAdLabelTag];
    if (!adLabel || ![adLabel isKindOfClass:[UILabel class]]) {
        adLabel = [[UILabel alloc] init];
        adLabel.tag = FsnAdLabelTag;
        adLabel.userInteractionEnabled = NO;
        adLabel.text = @"Ad";
        adLabel.textColor = [UIColor whiteColor];
        adLabel.font = [UIFont boldSystemFontOfSize:11.0];
        adLabel.textAlignment = NSTextAlignmentCenter;
        adLabel.backgroundColor = [[UIColor blackColor] colorWithAlphaComponent:0.55];
        adLabel.layer.cornerRadius = 4.0;
        adLabel.clipsToBounds = YES;
        [adView addSubview:adLabel];
    }

    adLabel.frame = CGRectMake(12.0, 10.0, 28.0, 18.0);

    // Z-order: card background dưới, asset trên, ad label trên cùng.
    [adView bringSubviewToFront:cardBg];
    [adView bringSubviewToFront:icon];
    [adView bringSubviewToFront:head];
    [adView bringSubviewToFront:media];
    [adView bringSubviewToFront:body];
    [adView bringSubviewToFront:cta];
    [adView bringSubviewToFront:adLabel];

    NSLog(@"[FsnAdIOS][LayoutStyle4AndroidLike] bounds=%@ | card=%@ | icon=%@ | head=%@ | media=%@ | body=%@ | cta=%@ | adLabel=%@",
          NSStringFromCGRect(b),
          NSStringFromCGRect(cardBg.frame),
          NSStringFromCGRect(icon.frame),
          NSStringFromCGRect(head.frame),
          NSStringFromCGRect(media.frame),
          NSStringFromCGRect(body.frame),
          NSStringFromCGRect(cta.frame),
          NSStringFromCGRect(adLabel.frame));
}

- (void)fsnForceAspectFitForSubviews:(UIView *)view {
    if (!view) {
        return;
    }

    view.clipsToBounds = YES;
    view.contentMode = UIViewContentModeScaleAspectFit;

    for (UIView *subview in view.subviews) {
        subview.clipsToBounds = YES;
        subview.contentMode = UIViewContentModeScaleAspectFit;

        if ([subview isKindOfClass:[UIImageView class]]) {
            UIImageView *imageView = (UIImageView *)subview;
            imageView.contentMode = UIViewContentModeScaleAspectFit;
            imageView.clipsToBounds = YES;
        }

        [self fsnForceAspectFitForSubviews:subview];
    }
}

- (UIImage *)fsnLoadImageNamed:(NSString *)name {
    UIImage *image = [UIImage imageNamed:name];

    if (!image) {
        NSLog(@"[FsnAdIOS][Image] Missing image: %@. Check iOS bundle / Copy Bundle Resources.", name);
    } else {
        NSLog(@"[FsnAdIOS][Image] Loaded image: %@ size=%@", name, NSStringFromCGSize(image.size));
    }

    return image;
}

- (void)fsnCleanNativeAdViewTree:(UIView *)view {
    if (!view) {
        return;
    }

    if ([view isKindOfClass:[GADNativeAdView class]]) {
        GADNativeAdView *nativeAdView = (GADNativeAdView *)view;

        if (nativeAdView.mediaView) {
            nativeAdView.mediaView.mediaContent = nil;
        }

        nativeAdView.nativeAd = nil;
        nativeAdView.headlineView = nil;
        nativeAdView.bodyView = nil;
        nativeAdView.callToActionView = nil;
        nativeAdView.iconView = nil;
        nativeAdView.mediaView = nil;
    }

    for (UIView *subview in [view.subviews copy]) {
        [self fsnCleanNativeAdViewTree:subview];
    }
}

- (void)fsnRemoveContainerView:(UIView *)container reason:(NSString *)reason {
    if (!container) {
        return;
    }

    NSLog(@"[FsnAdIOS][RemoveContainer] reason=%@ | container=%@ | frame=%@ | superview=%@",
          reason ?: @"",
          container,
          NSStringFromCGRect(container.frame),
          container.superview);

    container.hidden = YES;
    container.userInteractionEnabled = NO;

    [self fsnCleanNativeAdViewTree:container];

    for (UIView *subview in [container.subviews copy]) {
        [subview removeFromSuperview];
    }

    [container removeFromSuperview];
}

- (void)fsnRemoveAllVisibleNativeContainersExceptRequestId:(int)requestId {
    NSArray<NSNumber *> *keys = [self.nativeStates.allKeys copy];

    for (NSNumber *key in keys) {
        FsnAdStateIOS *state = self.nativeStates[key];

        if (state && state.adContainerView) {
            NSLog(@"[FsnAdIOS][FallbackRemove] remove state requestId=%d",
                  state.requestId);

            [self fsnRemoveContainerView:state.adContainerView
                                  reason:@"fallback-state-container"];

            if (state.countDownTimer) {
                [state.countDownTimer invalidate];
                state.countDownTimer = nil;
            }

            if (state.nativeAd.mediaContent.videoController) {
                state.nativeAd.mediaContent.videoController.delegate = nil;
            }

            state.nativeAd.delegate = nil;
            state.nativeAd = nil;
            state.adLoader = nil;
            state.isLoading = NO;
            state.adContainerView = nil;

            [self.nativeStates removeObjectForKey:key];
        }
    }

    UIViewController *rootVC = UnityGetGLViewController();
    UIView *rootView = rootVC ? [self fsnTopParentViewFromRootViewController:rootVC] : nil;

    if (!rootView) {
        return;
    }

    NSArray<UIView *> *subviews = [rootView.subviews copy];

    for (UIView *subview in subviews) {
        if (subview.tag == FsnNativeContainerTag) {
            NSLog(@"[FsnAdIOS][FallbackRemove] remove orphan container from rootView. frame=%@",
                  NSStringFromCGRect(subview.frame));

            [self fsnRemoveContainerView:subview
                                  reason:@"fallback-orphan-container"];
        }
    }
}

- (void)fsnClearNativeState:(FsnAdStateIOS *)state {
    if (!state) {
        return;
    }

    if (state.countDownTimer) {
        [state.countDownTimer invalidate];
        state.countDownTimer = nil;
    }

    if (state.nativeAd.mediaContent.videoController) {
        state.nativeAd.mediaContent.videoController.delegate = nil;
    }

    state.nativeAd.delegate = nil;
    state.nativeAd = nil;
    state.adLoader = nil;
    state.isLoading = NO;
    state.adContainerView = nil;
    state.shouldCloseAfterAdClick = NO;
}

- (void)fsnClearVisibleNativeStatesExceptRequestId:(int)preserveRequestId {
    NSArray<NSNumber *> *keys = [self.nativeStates.allKeys copy];

    for (NSNumber *key in keys) {
        int requestId = [key intValue];
        if (requestId == preserveRequestId) {
            continue;
        }

        FsnAdStateIOS *state = self.nativeStates[key];
        if (!state || !state.adContainerView) {
            continue;
        }

        NSLog(@"[FsnAdIOS][StateCleanup] Remove visible old state requestId=%d, preserve=%d",
              state.requestId,
              preserveRequestId);

        [self fsnClearNativeState:state];
        [self.nativeStates removeObjectForKey:key];
    }
}

- (void)fsnForceRemoveAllFsnNativeViewsFromRootOnly {
    UIViewController *rootVC = UnityGetGLViewController();
    UIView *rootView = rootVC ? [self fsnTopParentViewFromRootViewController:rootVC] : nil;

    if (!rootView) {
        NSLog(@"[FsnAdIOS][ForceRemoveOnly] rootView nil.");
        return;
    }

    NSLog(@"[FsnAdIOS][ForceRemoveOnly] Begin scan rootView=%@ | subviews=%lu",
          rootView,
          (unsigned long)rootView.subviews.count);

    [self fsnForceRemoveTaggedViewsInView:rootView];

    NSLog(@"[FsnAdIOS][ForceRemoveOnly] Done.");
}

- (void)fsnForceRemoveTaggedViewsInView:(UIView *)view {
    if (!view) {
        return;
    }

    NSArray<UIView *> *subviews = [view.subviews copy];

    for (UIView *subview in subviews) {
        [self fsnForceRemoveTaggedViewsInView:subview];

        if (subview.tag == FsnNativeContainerTag || subview.tag == FsnNativeAdViewTag) {
            NSLog(@"[FsnAdIOS][ForceRemove] Remove tagged view. tag=%ld | view=%@ | frame=%@ | superview=%@",
                  (long)subview.tag,
                  subview,
                  NSStringFromCGRect(subview.frame),
                  subview.superview);

            subview.hidden = YES;
            subview.alpha = 0.0;
            subview.userInteractionEnabled = NO;

            [self fsnCleanNativeAdViewTree:subview];

            for (UIView *child in [subview.subviews copy]) {
                [child removeFromSuperview];
            }

            [subview removeFromSuperview];
        }
    }
}

- (void)fsnForceRemoveAllFsnNativeViewsFromRoot {
    UIViewController *rootVC = UnityGetGLViewController();
    UIView *rootView = rootVC ? [self fsnTopParentViewFromRootViewController:rootVC] : nil;

    if (!rootView) {
        NSLog(@"[FsnAdIOS][ForceRemove] rootView nil.");
        return;
    }

    NSLog(@"[FsnAdIOS][ForceRemove] Begin scan rootView=%@ | subviews=%lu",
          rootView,
          (unsigned long)rootView.subviews.count);

    [self fsnForceRemoveTaggedViewsInView:rootView];

    NSArray<NSNumber *> *keys = [self.nativeStates.allKeys copy];

    for (NSNumber *key in keys) {
        FsnAdStateIOS *state = self.nativeStates[key];

        if (!state) {
            continue;
        }

        if (state.countDownTimer) {
            [state.countDownTimer invalidate];
            state.countDownTimer = nil;
        }

        if (state.nativeAd.mediaContent.videoController) {
            state.nativeAd.mediaContent.videoController.delegate = nil;
        }

        state.nativeAd.delegate = nil;
        state.nativeAd = nil;
        state.adLoader = nil;
        state.isLoading = NO;
        state.adContainerView = nil;

        [self.nativeStates removeObjectForKey:key];
    }

    NSLog(@"[FsnAdIOS][ForceRemove] Done.");
}

@end

// MARK: - C BRIDGE LINKING EXTERNAL TO UNITY

extern "C" {

    void FsnAdIOS_SetCallbacks(IOSLoadingCompletedDelegate onCompleted,
                               IOSLoadingStartedDelegate onStarted,
                               IOSAdPaidDelegate onPaid,
                               IOSAdCompletedDelegate onAdCompleted) {

        _unityOnLoadingCompleted = onCompleted;
        _unityOnLoadingStarted = onStarted;
        _unityOnAdPaid = onPaid;
        _unityOnAdCompleted = onAdCompleted;
    }

    void FsnAdIOS_InitNative(int requestId,
                             int type,
                             int ratio,
                             const char* adUnitId,
                             int countDownSec,
                             long delayForCountDown) {

        NSString *nsAdUnitId = adUnitId ? [NSString stringWithUTF8String:adUnitId] : @"";

        [[FsnAdIOS sharedInstance] initNativeWithRequestId:requestId
                                                      type:type
                                                mediaRatio:ratio
                                                  adUnitId:nsAdUnitId
                                              countDownSec:countDownSec
                                         delayForCountDown:delayForCountDown];
    }

    void FsnAdIOS_LoadNativeAd(int requestId, int ratio) {
        [[FsnAdIOS sharedInstance] loadNativeAdWithRequestId:requestId
                                                  mediaRatio:ratio];
    }

    void FsnAdIOS_ShowNativeAd(int requestId,
                               int layoutType,
                               int overlayOpenStorePos,
                               int overlayClosePos) {

        [[FsnAdIOS sharedInstance] showNativeAdWithRequestId:requestId
                                                  layoutType:layoutType
                                         overlayOpenStorePos:overlayOpenStorePos
                                             overlayClosePos:overlayClosePos];
    }

    void FsnAdIOS_HideNativeAd(int requestId) {
        [[FsnAdIOS sharedInstance] hideNativeAdWithRequestId:requestId];
    }

    bool FsnAdIOS_IsNativeAdReady(int requestId) {
        return [[FsnAdIOS sharedInstance] isNativeAdReadyWithRequestId:requestId];
    }
}