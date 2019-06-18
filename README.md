# FormSystem_Partial
A system for managing forms. Developed in <b>C#.NET MVC</b>.<br />
<b>LINQ</b> is used to update (i.e. not <b>SQL</b>) the database which is a <b>MSSQL</b> one.<br />
Only this part of the code is free to view.<br />
If you want to view more code or buy the system, please contact me.<br />
It is possible to convert this code to <b>C#.NET Core</b>, of course !<br />
UI is in English.
<p />
<b>The code is a good example of the programming principles of</b><br />
<b>1)</b> <b>ViewModel:</b><br />
&nbsp;&nbsp;&nbsp; in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Models/IdeaCarrier/Startup.cs<br />
&nbsp;&nbsp;&nbsp; row 166 and below.<br />
<b>2)</b> <b>virtual:</b><br />
&nbsp;&nbsp;&nbsp; see https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
&nbsp;&nbsp;&nbsp; rows 719 and 720 (virtual takes care of that modification in the database method SaveChanges).<br />
<b>3)</b> <b>Inheritence:</b><br />
&nbsp;&nbsp;&nbsp; Can also be used in ViewModels (and in Models of course !).<br />
&nbsp;&nbsp;&nbsp; In https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Models/IdeaCarrier/Startup.cs<br />
&nbsp;&nbsp;&nbsp; row 284 and below.<br />
&nbsp;&nbsp;&nbsp; (What do you do if you want more than 1 database table fram the same model in ORM ? Simply use inheretence !)<br />
<b>4)</b> <b>No unnecessary updates of the database:</b><br />
&nbsp;&nbsp;&nbsp; in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
&nbsp;&nbsp;&nbsp; Database method SaveChanges (rows 255,331,1066,1133,1175,1202,1239,1273) and<br />
&nbsp;&nbsp;&nbsp; EntityState method Modified (rows 327,1065,1132,1201,1238,1272).<br />
&nbsp;&nbsp;&nbsp; are only used when really necessary !<br />
<b>5)</b> <b>Code easier to read:</b><br />
&nbsp;&nbsp;&nbsp; in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
&nbsp;&nbsp;&nbsp; using the pattern: var X = GetX(/*parameters*/). Rows 363,388,392.<br />
&nbsp;&nbsp;&nbsp; No use of Viewbags (or ViewData), except in Index (rows 28-114) (and ProjectDetails (rows 118-156),<br />
&nbsp;&nbsp;&nbsp; using ViewModels with SelectLists instead.<br />
&nbsp;&nbsp;&nbsp; Rows 202,204,267,269,358,367,369,378,394,396.<br />
<b>6)</b> <b>LINQ and ORM</b>:<br />
&nbsp;&nbsp;&nbsp; e.g. in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
&nbsp;&nbsp;&nbsp; e.g. Rows 56,61,76,81,133,149.<br />
<b>7)</b> <b>Razor:</b><br />
&nbsp;&nbsp;&nbsp; e.g. in the cshtml files: https://github.com/StefLove/FormSystem_Partial/tree/master/EoS/Views
<p />
<b>Major differences between the old and the new code</b><br />
Look at https://github.com/StefLove/FormSystem_Partial/edit/master/EoS/Controllers/StartupsController_bad_old_code<br />
<b>1)</b> That horrible [Bind(Include = "...")] (which makes the code less readable and has that problem with overposting attacks)<br />
&nbsp;&nbsp;&nbsp; rows 162, 254.<br />
&nbsp;&nbsp;&nbsp; The code gets much better with ViewModels as in rows 325-332 and 340-358.<br />
<b>2)</b> ViewBag after ViewBag (horrible too)<br />
&nbsp;&nbsp;&nbsp; 139-151, 197-207, 223-233, 294-303<br />
Compare with new code https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController.cs<br />
<b>Most of the improvements:</b><br />
<b>1)</b> Instead of [Bind(Include = "...")] ViewModels are used (overposting attacks are no problem with those):<br />
&nbsp;&nbsp;&nbsp; The code is much easier to read now.<br />
&nbsp;&nbsp;&nbsp; Rows 216, 316<br />
<b>2)</b> Viewbags are only used in Index (and in ProjectDetails, but that is easy to change):<br />
&nbsp;&nbsp;&nbsp; Rows 28-114, 118-156 (see also above under 5)).<br />
<b>3)</b> Names has been renamed and is now easier to understand because they follow a logic:<br />
&nbsp;&nbsp;&nbsp; Details (old code row 110) to ProjectDetails (row 118)<br />
&nbsp;&nbsp;&nbsp; Create (old code rows 137, 162)  to AddNewProject (rows 196, 216)<br />
&nbsp;&nbsp;&nbsp; Edit (old code rows 214, 254) is renamed to ProjectForm (rows 277, 316)<br />
&nbsp;&nbsp;&nbsp; Delete (old code row 362) to RemoveProject (row 1144)<br />
<b>4)</b> This new code is more effective and thus runs faster mainly due to the splitting of the ProjectForm into 5 parts:<br />
&nbsp;&nbsp;&nbsp; Have a look at the files containing the word ProjectForm in<br />
&nbsp;&nbsp;&nbsp; https://github.com/StefLove/FormSystem_Partial/tree/master/EoS/Views/Startups<br />
&nbsp;&nbsp;&nbsp;<br />
<b>5)</b> Unnessary code deleted:<br />
&nbsp;&nbsp;&nbsp; in https://github.com/StefLove/FormSystem_Partial/blob/master/EoS/Controllers/StartupsController_bad_old_code<br />
&nbsp;&nbsp;&nbsp; rows<br />
<b><6/b> Simpification using only Lists, in the old code HashSets were used (without motivation).<br />
&nbsp;&nbsp;&nbsp; 
<p />
I have experienced that some developers write worse code than this,<br />
despite an education of 5 years in IT technology and programming !<br />
You just can't be a plain developer, you must be an excellent developer who delivers code that works and looks nice !<p />
Stefan<br />
Stockholm<br />
Sweden
