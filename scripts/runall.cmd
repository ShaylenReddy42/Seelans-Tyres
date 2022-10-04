@ECHO off

CD "%~dp0"

CD ..\publish

START /min CMD /c SeelansTyres.Frontends.Mvc\run.cmd

START /min CMD /c SeelansTyres.Gateways.MvcBff\run.cmd

START /min CMD /c SeelansTyres.Services.AddressService\run.cmd

START /min CMD /c SeelansTyres.Services.IdentityService\run.cmd

START /min CMD /c SeelansTyres.Services.OrderService\run.cmd

START /min CMD /c SeelansTyres.Services.TyresService\run.cmd

ECHO Please wait 10 seconds for the applications to warm up to launch the website

TIMEOUT /T 10 /NOBREAK

START http://localhost:5001
