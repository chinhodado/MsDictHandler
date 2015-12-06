A tool to turn a MsDict .dict file into an SQLite database.

 - Change the name of the dict inside MainPage.xaml.cs.
 - Run MsDictHandler once, just to get it installed on the machine. Close the app, and make sure the MsDictHandler.exe process is killed.
 - Copy the DB_files folder into bin\x86\Debug\AppX\. Inside DB_files\ is the dict file to convert.
 - In AppData\Local\Packages\28587606-2a78-4929-9f33-7a8c1e7102f7_bhmrb0r2ehb8t\LocalState\, make sure the test.db file is in there, and that
 it contains a table named WordTable with 2 columns Word and Definition. If you have run MsDictHandler once this db file with the table should
 be there.
 - Delete all rows from the table, i.e. "delete from WordTable"
 - Run MsDictHandlerContinuer, let it run until MsDictHandler says "All done!" and no longer restarting.
 - Close MsDictHandlerContinuer first, then close MsDictHandler
 - The finished db file is in AppData\Local\Packages\28587606-2a78-4929-9f33-7a8c1e7102f7_bhmrb0r2ehb8t\LocalState\