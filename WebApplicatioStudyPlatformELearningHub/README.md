# Database Restore Instructions

To restore the database from the provided backup file, follow these steps:

1. Open SQL Server Management Studio (SSMS).
2. Connect to your local SQL Server instance.
3. Right-click on the 'Databases' folder and select 'Restore Database.
4. In the 'Restore Database' dialog, choose 'Device' and click the button to browse.
5. Navigate to the `.bak` file located in the `DatabaseBackup` directory of this project.
6. Select the file and click 'OK'.
7. Verify the restore options and click 'OK' again to start the restoration process.

Please note that you must have permissions to restore databases on the SQL Server instance.
