# FormSystem_Partial
A system for managing forms. Developed in <b>C#.NET MVC</b>.<br />
<b>LINQ</b> is used to update (i.e. not <b>SQL</b>) the database which is a <b>MSSQL</b> one.<br />
Only this part of the code is free to view.<br />
If you want to view more code or buy the system, please contact me.<br />
It is possible to convert this code to <b>C#.NET Core</b>, of course !<br />
UI is in English.
<p />
The code is a good example of the programming principles of<br />
1) <b>ViewModel:</b><br />
&nbsp;&nbsp;&nbsp; in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Models/IdeaCarrier/Startup.cs<br />
&nbsp;&nbsp;&nbsp; row 166 and below.<br />
2) <b>virtual:</b><br />
&nbsp;&nbsp;&nbsp; see https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
&nbsp;&nbsp;&nbsp; rows 719 and 720 (virtual takes care of that modification in the database method SaveChanges).<br />
3) <b>Inheritence:</b><br />
&nbsp;&nbsp;&nbsp; Can also be used in ViewModels (and in Models of course !).<br />
&nbsp;&nbsp;&nbsp; In https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Models/IdeaCarrier/Startup.cs<br />
&nbsp;&nbsp;&nbsp; row 284 and below.<br />
&nbsp;&nbsp;&nbsp; (What do you do if you want more than 1 database table fram the same model in ORM ? Simply use inheretence !)<br />
4) <b>No unnecessary updates of the database:</b><br />
&nbsp;&nbsp;&nbsp; in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
&nbsp;&nbsp;&nbsp; Database method SaveChanges (rows 255,331,1073,1149,1191,1218,1255,1381) and<br />
&nbsp;&nbsp;&nbsp; EntityState method Modified (rows 327,1072,1148,1217,1254,1380)<br />
&nbsp;&nbsp;&nbsp; are only used when really necessary !<br />
5) <b>Code easier to read:</b><br />
&nbsp;&nbsp;&nbsp; in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
&nbsp;&nbsp;&nbsp; using the pattern X = GetX(/*parameters*/). Rows 363,388,392.<br />
&nbsp;&nbsp;&nbsp; no use of Viewbags (or ViewData), except in Index files: using ViewModels and SelectLists instead.<br />
&nbsp;&nbsp;&nbsp; Rows 202,204,267,269,358,367,369,378,394,396.
<p />
I have experienced that some developers write worse code than this,<br />
despite an education of 5 years of IT technology and programming !


Stefan
Stockholm
Sweden
