# Data Security – SharpStack

**Effective date:** August 29, 2026

This page explains practical data security measures for SharpStack.

## 1. Security principles

SharpStack follows these principles:

- Data minimization
- Least-privilege access
- Separation of concerns between app logic and third-party services
- Defense-in-depth for release processes

## 2. Data scope

The game is primarily local-first in behavior.  
Potential sensitive processing is mainly tied to external SDK/services (for example advertising providers such as AdMob, if enabled in production).

## 3. Technical safeguards

Where applicable, the publisher aims to use:

- Official SDK integrations from trusted providers
- Secure transport (HTTPS/TLS) for network communications
- Up-to-date dependencies and SDK versions
- Platform security features offered by Android
- Access control over release keys, store credentials, and repository administration

## 4. Organizational safeguards

- Limited access to publishing accounts
- Controlled release workflow
- Review before introducing new third-party SDKs
- Periodic review of Play Console data disclosure settings

## 5. Incident handling

If a security issue is identified, the publisher may:

1. Investigate and scope impact  
2. Patch and release mitigation updates  
3. Update disclosures and notify users/platforms where required by law or policy

## 6. Third-party security

When AdMob or other Google services are enabled, relevant security controls and retention are additionally governed by Google infrastructure and policy.

## 7. User actions for better safety

Users can improve privacy/security by:

- Keeping Android and the app updated
- Resetting advertising ID (where available)
- Reviewing device privacy settings
- Uninstalling the app to remove local app data
