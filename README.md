# Raba
Windows Service Application

Release Note version 1.0.0
- Upgrade legacy source code to .net framework 4.6.1
- RABA GUI change.
- RABA Service handle for ZIP, UNZIP command.
- Added Wix project as installer for Raba GUI and Raba Service.
- The installation file placed in folder RabaSetup

Testing note
- Verify Zip, Unzip with large file (~300mb) take arround 20s to process.

Release Note version 1.1.0
- Add Task
- Add/Edit/Delete Macro file
- RABA Service handle for COPY, DELETE, MOVE, BATCH, SQL SCRIPT command
- Add Credential to installation package

Release Note version 1.2.0 - Apr 22, 2021
- Add RUN action to RABA control
- Add "RUN" to RABA file

Release Note version 1.2.1 - Apr 27, 2021
- Change default size to 1366x730
- Update main window
	+ Make enable macro panel bigger
	+ Make disable macro panel smaller

Release Note version 1.2.2 - May 08, 2021
- Add Appname, Version and Copyright to About page
- Fix issue: Raba crashed when adding the same Run Action into a macro

Release Note version 5.0.0.1 - May 21, 2021
- Add Conditional Delete action
- Add About page
- Write log to file. Log file store by date folowwing format log-MMddyyyy.txt

Release Note version 5.0.0.2 - June 08, 2021
- Restructure settings screen layout
- Add Flyout Manage Raba file
- Add action detail panel



