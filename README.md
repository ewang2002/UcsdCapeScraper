# UCSD Cape Scraper
A simple program designed to scrape data from [UCSD's CAPE](http://www.cape.ucsd.edu/). 

## Technologies
- .NET 6.0
- Selenium

## Requirements
- [Google Chrome](https://www.google.com/chrome/)
- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- [Selenium Driver for Chrome](https://chromedriver.storage.googleapis.com/index.html)

## Usage 
The command line arguments are:

```
./UcsdCapeScraper -d <Directory> -u <TritonLink Username>
./UcsdCapeScraper -dir <Directory> -username <TritonLink Username>
```

Where:
- `<Directory>` (Required) is the directory that contains your `chromedriver.exe` file.
- `<TritonLink Username>` (Optional) is the username that you want to use to login. 

Once you run this program, the program will ask for your password. Follow the directions. 

## Instructions
At some point, I will provide a compiled executable. For now, you will need to compile `UcsdCapeScraper` yourself. More on this will be provided. 

Once you've followed the steps that the program has given you, the program will scrape data from the CAPE website. After that is done, it will save a TSV file in the same directory where you specified that the `chromedriver.exe` executable was.

## License
MIT.