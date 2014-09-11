PictureSelfService
==================

##General

The PictureSelfService allows users to upload and modify pictures. It is implemented as web-component for the Identity- and Access-Management system "[Quest One Identity Manager™](http://www.quest.com/identity-manager/)" (further called QOIM).
Main purpose of this component is to provide end users with an easy way to upload and optimize profile photos (crop and alter contrast & brightness) for distribution through the IAM - for example publish to [Active Directory](https://en.wikipedia.org/wiki/Active_Directory).

##Content

The package consists of a .NET assembly providing basic image manipulation functions (utilizing [QColorMatrix](http://www.codeguru.com/Cpp/G-M/gdi/gdi/article.php/c3667) and some XML files that contains the web-component definitions to bring this power into QOIM.
To compile and run the .NET module, the libraries of QOIM (currently version 6.0) are required (can be found in the installation directory of local QOIM deployment). The XML-Files contains the Web-Component definitions that can be imported using the WebDesigner™ Tool of QOIM.

##Licensing

The code is available at [github](home) under [LGPL3](https://www.gnu.org/licenses/lgpl.html).

We encourage you to use and enhance this code and to share your achievements - especially when adopting new QOIM versions (as they likely contain breaking changes in the undocumented API).
