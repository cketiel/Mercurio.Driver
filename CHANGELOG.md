# Changelog

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
The record starts at version `1.1.0`; earlier history is not reconstructed.

## [1.2.0] - 2026-08-27

### Added
- Notifications reach the driver at last. The backend had been producing them for a while,
  but the app had no way to receive them: it now has an inbox, a live channel and push.
- Bell with an unread counter in the navigation bar, and a NOTIFICATIONS entry in the menu.
- Notifications screen: pull to refresh, mark as read and unread, mark all as read, and hide
  a notification. Hiding is local to the phone — the record stays on the server and is removed
  by the retention policy, never by the app.
- Push notifications through Firebase. Tapping one opens the app on the notifications screen.
- The app reacts to route changes while the driver is working: when a trip is added to the
  route, taken off it, or cancelled, the schedule on screen is corrected. The driver is told
  what happened and why the screen is about to change.
- Drivers can now call and text the patient from a future trip's detail.

### Changed
- Page titles are centred.
- Push notifications show the Raphael icon instead of a blank square.

### Fixed
- ETA calculation no longer schedules a driver to reach a pickup more than fifteen minutes
  early, which is the limit the business rule allows.
- Signing out closes the side menu instead of leaving it open over the login screen.
- "Copy phone number" copied the address.
- Signing out now unregisters the device, so the next driver to use the same phone does not
  receive the previous one's notifications.

## [1.1.0] - 2026-08-11

### Added
- The application version is shown on screen.

### Changed
- Flyout menu styles were standardised.
