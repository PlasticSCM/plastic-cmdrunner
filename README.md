plastic-cmdrunner
=================

Utility to run Plastic SCM CLI "cm" commands from a different application (useful for plugins and scripts).

This project requires Visual Studio 2010 (or Visual Studio 2012) and .NET Framework 2 to compile.

The code consists of:
* The CmdRunner (command runner) itself. This utility allows you to run cm commands from a .NET program,
  thus allowing writing connectors between Plastic SCM and other .NET-based applications. This application
  run cm commands in different process and parses the output obtained. You can run separate commands such as:
  
  > cm find branches
  
  Or within a shell context, such as:
  > cm shell
  > find branches
  
  The main different between both uses is that the cm shell is faster when running several commands in a row,
  since the cm.exe application is already launched. In most cases this is the desired behaviour, since it is
  much better from the performance point of view. The counterpart of using this is that if some configuration
  happens (i.e.: the Plastic SCM server is changed in the client.conf), then the cm shell must be restarted to
  load the new changes.

* CmdRunnerExample: This is an example of a very simple application that executes several common Plastic SCM
  commands and get the result in different ways: get the string result, get the command result (0 means success,
  other value means error). In order to test this example you need to have the "cm.exe" in your path and
  configured with a Plastic server that must be running.
