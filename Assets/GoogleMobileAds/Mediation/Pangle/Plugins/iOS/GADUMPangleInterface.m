// Copyright 2022 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#import <PangleAdapter/PangleAdapter.h>

void GADUMPangleSetGDPRConsent(int GDPR) {
  [GADMediationAdapterPangle setGDPRConsent:GDPR];
}

// Pangle adapter 7.1.1.0.1 renamed +setDoNotSell: to +setPAConsent:, so the old
// selector no longer exists in the 7.9.1.1.1 pod this project pins and calling it
// fails to compile. The C symbol name is kept because Externs.cs binds to it.
// Polarity is inverted between the two APIs: doNotSell=1 (opted out of sale) is
// equivalent to PAConsent=0 (no consent for personalized ads).
// NOTE: nothing in this project calls Pangle.SetDoNotSell, so this path is unused.
// Reimporting the GoogleMobileAds Pangle plugin resets this file -> re-apply.
void GADUMPangleSetDoNotSell(int doNotSell) {
  [GADMediationAdapterPangle setPAConsent:(doNotSell != 0 ? 0 : 1)];
}
