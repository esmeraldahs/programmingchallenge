# Programming Challenge

###### Hello PrestaCap, and welcome in this programming challenge
###### My name is Esmeralda Hysenaj and below you can find notes and documentation regarding the tasks.

In the solution named **ProgrammingChallenge**, you will find the solution for 2 out of 3 tasks.
if you're wondering why 2, then the reason is i want to stand out :)
Okay i will be honest, despite how much i want to stand out and be selected, that's not the main reason why. 
When i first got the email, i went straight to the link with the tasks because of excitment and started developing, assuming i had to work on all 3 tasks. 
After working on first 2 ones, i had to return to the email and figured out i only had to **choose 1**!!!
Since the development was almost over for the first 2 ones, i decided to keep going like that.

Reason why there are 2 tasks solutions in one project : to not make the solution too heavy and to have only *one working web app with multiple functionalities*.

###### Documentation

1. Task nr 1 :  Web Extraction 
    * Get's the html file, which is included in the solution folder
	* Extracts the required data
	* Shows the data in a view and gives you the option to download the json file
2. Task nr.2 : Reporting
	* Get's json file  *(hotelrates.json)*
	* Deserializes the data & gets what we need
	* Returns and downloads locally the excel report file
3. For both of tasks to work properly, the html file called *"bookingpage.html"* and json file called *"hotelrates.json"* should be included in project's folder (currently they are included)

1. Frameworks and technologies used: 
	* .NET Framework version 4.7.1
	* ASP.NET MVC version 5.2.7
	* MSTest for UnitTesting
	* log4net for logging 
	* HtmlAgilityPack for reading data from an html file
	* ClosedXML to create the excel file
	* Newtonsoft.Json for working with json files	
2. logs file path = `{Current directory of the project}\logs\`


###### Option task at task number 2 
> Architecture suggestion:
1. Windows service or console application (running on background)
	* Get report file from local path
	* Compose the email and attach the file
	* Send the email
2. Windows task scheduler to run task (.exe file of the application) at a specific time 

> Frameworks and technologies: 
1. .NET Framework version 4.7.1
2. Windows service or console application
2. System.Net.Mail for composing and sending the email
3. Windows Task Scheduler for running the app