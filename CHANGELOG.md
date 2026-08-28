# Changelog

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
The record starts at version `1.1.0`; earlier history is not reconstructed.

## [1.3.0] - 2026-08-27

### Added
- Every page title carries the icon it already had in the side menu.
- A route change now shows up in the bell like any other notification, so the driver can see
  what moved their schedule instead of only watching it move.

### Changed
- Future Schedule shows the **next day only**. It used to list every day ahead in a single
  list, with several Pull-outs and several Pull-ins in it and nothing to tell the days apart.
- A route change only interrupts a screen that was already open when it arrived, and only
  while it is less than an hour old. Every screen loads current data as it opens: being asked
  to reload what you have just loaded is noise.
- Settings uses the same title bar as every other page, bell included.

### Fixed
- The route-change overlay and its countdown never appeared. The schedule reloaded five
  seconds later with nothing on screen to explain why: the overlay was built off the UI
  thread and the failure only reached the debug log.
- Future Schedule was a dead list. No event could be opened, so the calls and texts added in
  1.2.0 could never be reached. An event opens now and offers exactly two actions: call the
  patient and text them.
- The bell was missing from Today's Schedule and Future Schedule - the two screens where a
  driver spends the shift, and so the two where a new notification went unnoticed.
- Page titles sat right of centre. Android places the title view after the navigation icon,
  so a title centred inside it is not centred on the screen.
- The "no trips" message is centred on both schedule screens.

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
