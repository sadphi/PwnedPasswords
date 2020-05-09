# PwnedPasswords
A small .NET Core program to check if a password has been leaked by using Have I Been Pwned.


## Usage
Note: The program is currently only tested for Windows, but should work on all supported .NET Core platforms

### Windows
Open a command line in the same directory as the `pwnedpasswords.exe` file and enter:
```batchfile
pwnedpasswords.exe [Password]
```
Where [Password] is the password to check (If the password contains spaces, it must be surrounded with quotation marks, e.g. "Password With Spaces")


#### Example
For example, typing:
```batchfile
pwnedpasswords.exe password
```
Will output:
```batchfile
This password has been pwned, and it has been seen 3 730 471 times!
Pwned Hash: 5BAA61E4C9B93F3F0682250B6CF8331B7EE68FD8
Pwned Password: password
```

Currently the program only accepts one password as a launch argument, which means it won't do anything if nothing is specified. 

## To-Do
- [x] Check multiple passwords at a time
- [ ] Allow piping
