#import "PackageUtilHelper.h"
@implementation PackageUtilHelper
- (bool) check: (NSString *) scheme {
	NSString *schema = [NSString stringWithFormat:@"%@://",scheme];
    
    UIApplication *application = [UIApplication sharedApplication];
    NSURL *URL = [NSURL URLWithString:schema];
    if([application canOpenURL:URL]){
		return true;
	}
	
	//#pragma clang diagnostic push
	//#pragma clang diagnostic ignored "-Wundeclared-selector"
	//	NSData *data = [[NSData alloc] initWithBase64EncodedString:@"TFNBcHBsaWNhdGlvbldvcmtzcGFjZQ==" options:0];
	//	//Class c = objc_getClass([[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] cStringUsingEncoding:NSUTF8StringEncoding]);
    //NSString *className = [NSString stringWithUTF8String:[[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] cStringUsingEncoding:NSUTF8StringEncoding]];
    ////Class class = NSClassFromString((NSString *)className);
    //
    //Class c = NSClassFromString(className);
    //
	//	data = [[NSData alloc] initWithBase64EncodedString:@"ZGVmYXVsdFdvcmtzcGFjZQ==" options:0];
	//	NSObject* w = [c valueForKey:[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]];
	//	data = [[NSData alloc] initWithBase64EncodedString:@"YWxsQXBwbGljYXRpb25z" options:0];
	//	NSArray *aD = [w valueForKey:[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]];
	//	for(int i = 0; i < aD.count; i++){
	//		data = [[NSData alloc] initWithBase64EncodedString:@"YXBwbGljYXRpb25JZGVudGlmaWVy" options:0];
	//		NSString *b = [aD[i] valueForKey:[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]];
	//		if ([scheme isEqualToString:b]) {
	//			return YES;
	//		}
	//	}
	//#pragma clang diagnostic pop
	return false;
}
@end