# XFiles
XFiles is a super simple file sharing application written in c# using .net core 10.0, the application aims to facilitate p2p file transfer between two devices. 

INSTRUCTIONS FOR USE

The Program is a console-based peer-to-peer file transfer utility designed for operation within a trusted local network. Interaction with the Program is performed exclusively through text-based commands entered by the user.

Upon launching, the Program presents a command prompt. The user may type “help” to display a list of available commands and their corresponding aliases. Commands are not case-sensitive.

The primary operational commands are as follows:

Exit (alias: e): Terminates the Program immediately.
Send (alias: s): Initiates the transmission of a file to another device on the network. The user will be prompted to provide a recipient identifier (which may be an IP address, encoded device address, or stored nickname) and the full file path of the file to be transmitted.
Receive (alias: r): Places the Program into a listening state on the configured port (default: 9000) to accept an incoming file transfer. Received files are saved to the system’s default downloads directory.
Address (alias: a): Displays the user’s device address in encoded form. The user may choose between a local network address (LAN or wireless) or a public-facing address.
Nickname (alias: n): Allows the user to create, update, or remove aliases (nicknames) associated with device addresses. These nicknames may subsequently be used in place of raw addresses when sending files.
Options (alias: o): Displays and permits modification of configurable settings, including but not limited to network port, buffer size, and startup behavior.

Additional operational behaviors include:

Configuration Files:
The Program automatically creates and maintains configuration files within the user’s application data directory. These files store user-defined nicknames and operational settings.
Address Handling:
Device addresses may be stored and transmitted in encoded form. The Program will automatically resolve and decode such addresses when used in file transfer operations.
File Transfers:
File transfers occur over a designated network port and rely on direct peer-to-peer communication. Successful operation requires that both sending and receiving devices are accessible within the same network environment and that no external firewalls or restrictions block the configured port.
Error Handling:
In the event of invalid input, connection failure, or file errors, the Program will display an error message and return control to the user without terminating, unless explicitly instructed.
Reset and Restart Functions:
The Program includes commands to restart itself and to reset configuration files to default values. Use of such functions will overwrite existing configuration data.

Users are solely responsible for correctly entering commands, verifying file paths, and ensuring that recipient devices are reachable and authorized within the same trusted network.



**DISCLAIMER AND LIMITATION OF LIABILITY**

This software program (the “Program”) is provided on an “as-is” and “as-available” basis, without warranties of any kind, whether express or implied.

The Program is not designed or intended to incorporate advanced or comprehensive security features. It is expressly intended for use solely within a private, controlled network environment consisting of trusted devices. Users acknowledge and agree that the Program should not be exposed to untrusted networks, including but not limited to the public internet, without additional, independent security measures implemented by the user.

By using the Program, the user accepts full responsibility for ensuring that the network environment in which the Program operates is secure and appropriate for its intended use. The user further acknowledges that any modification of the intended use, including but not limited to connecting the Program to public or unsecured networks, is undertaken entirely at the user’s own risk.

To the fullest extent permitted by applicable law, the developer, distributor, and any associated parties shall not be held liable for any damages, losses, security breaches, data exposure, or other adverse consequences arising from or related to:
(a) the use or misuse of the Program;
(b) the deployment of the Program in an unsecured or untrusted environment; or
(c) the connection of the Program to the internet or other public networks.

No guarantees are made regarding the security, reliability, or fitness of the Program for any particular purpose beyond its stated intended use within a trusted local network.

By installing, accessing, or using the Program, the user signifies their understanding and acceptance of this disclaimer.
