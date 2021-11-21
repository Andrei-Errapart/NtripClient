Lefebure NTRIP Client
Send comments to Lance@Lefebure.com



Requirements: Microsoft .NET 2.0 Framework.



Installation Instructions: Just run the program. All settings are stored in files in the same directory as the application. Event logs are written in a subfolder /Logs to a file named YYYYMMDD.txt, where YYYY is the year, MM is the month, and DD is the date. Nothing is written to or read from the windows registry.



Purpose: This software is a NTRIP Client. It is intended to be used as a replacement for GNSS Internet Radio. It will connect to a NTRIP Caster on the Internet, get GPS (DGPS or RTK) correction data, and send that data to a serial port. If the caster requires knowing your position, the software will periodically report its current location. That location will typically come from your GPS receiver in the form of a standard NMEA GGA sentence, but can also be generated from a manually entered latitude and longitude.



Supporting Files:
Ntripconfig.txt – Contains NTRIP connection settings.
Settings.txt – Other settings.
Sourcetable.txt – This is the source table received from the NTRIP Caster.



File Example: ntripconfig.txt
NTRIP Caster=1.2.3.4
NTRIP Caster Port=10000
NTRIP Username=yourusername
NTRIP Password=yourpassword
NTRIP MountPoint=THEDATASTREAMYOUWANT



File Example: Settings.txt
Serial Port Number=1
Write Events to File=Yes
Serial Port Speed=38400
Serial Port Data Bits=8
Serial Port Stop Bits=1
Serial Should be Connected=Yes
NTRIP Use Manual GGA=Yes
NTRIP Manual Latitude=41
NTRIP Manual Longitude=-91
NTRIP Should be Connected=Yes
Audio Alert File=somewavefile.wav
NTRIP Only Send GGA Once=No
