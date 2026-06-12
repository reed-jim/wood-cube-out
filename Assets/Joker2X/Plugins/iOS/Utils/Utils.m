#import "PackageUtilHelper.h"
// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]
extern bool _checkInstalled(const char* message)
{
	PackageUtilHelper* helper = [[PackageUtilHelper alloc]init];
    return [helper check:GetStringParam(message)];
}