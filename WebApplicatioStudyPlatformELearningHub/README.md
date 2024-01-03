# Database Restore Instructions

To restore the database from the provided backup file, follow these steps:

1. Open SQL Server Management Studio (SSMS).
2. Connect to your local SQL Server instance.
3. Right-click on the 'Databases' folder and select 'Restore Database.
4. In the 'Restore Database' dialog, choose 'Device' and click the button to browse.
5. Navigate to the `.bak` file located in the `DatabaseBackup` directory of this project.
6. Select the file and click 'OK'.
7. Verify the restore options and click 'OK' again to start the restoration process.
8.	Download three folders from google drive and copy into project www.root folder.
https://drive.google.com/drive/folders/1wD9v1iagEWkqcNNhptqS3BA-kzic8lFl?usp=sharing



Github and .bak file instruction
1.	Open GitHub link -  https://github.com/TomashTom/StudyPlatformELearning 
2.	Click on (<> Code) and Download ZIP.
3.	Click on “DatabaseBackup”  and open file named “StudyPlatform.bak”.
4.	Click on “View raw”.  It will download “.bar” file and locate this file into Local Disc (C:).
5.	Go into Microsoft SQL Server Management Studio.
6.	Right-click on Databases and choose “Restore Database…”.
7.	In Restore Database in section Source choose “Device”.
8.	Click on three dots. 
9.	Click button “Add”.
10.	Find in Local Disc (C:) where did you saved “.bak” file and select it and click OK
11.	Open downloaded project in Visual Studio
12.	Go to file named “appsettings.json”
13.	In line:
"DataContextConnection":"Server=DESKTOPKMMOJUS;Database=StudyPlatform;
Change into your Server name. in my case it is (DESKTOPKMMOJUS).
