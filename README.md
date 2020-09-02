# UCSD Cape Scraper
A simple program designed to scrape data from [UCSD's CAPE](http://www.cape.ucsd.edu/). 

## Technologies
- .NET Core 3.1 
- Selenium

## Requirements
- [Google Chrome](https://www.google.com/chrome/)
- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)
- [Selenium Driver for Chrome](https://chromedriver.storage.googleapis.com/index.html)

## Instructions
You may choose to either run the pre-built (compiled) version of the program or build the program yourself.

⚠️ This program has only been tested using Windows. This program may not work on MacOS or Linux.

<details>
<summary>Pre-Built (Compiled)</summary>
<br>

**NOTE**: I have not formally released this project yet. 
1. Go to the project's [releases page](https://github.com/ewang2002/UcsdCapeScraper/releases).
2. Download the latest release.
3. Fill out the `SAMPLE_CONFIG.txt` file and rename it to `config.txt`. 
4. Run the program directly.
</details>

<details>
<summary>Building It Yourself</summary>
<br>

1. Download the project.
	- If you have [Git](https://git-scm.com/downloads), run this command.
	```
	git clone https://github.com/ewang2002/UcsdCapeScraper.git
	```

	- If you do not have Git, just [download the source code](https://github.com/ewang2002/UcsdCapeScraper/archive/master.zip).
	
2. Install the items above (under "Requirements"). These are required. 
	- For the Selenium Driver, pick the version that best matches your Chrome's version (Go to Google Chrome and go to `chrome://version/`). 
2. Go to the `SAMPLE_CONFIG.txt` file, rename it to `config.txt`, and fill it out. 
3. Open your command line tool of choice and make sure you are in your root directory. The root directory is the directory that has files such as `README.md`, `SAMPLE_CONFIG.txt`, and `UcsdCapeScraper.sln`. 
4. Run the following commands.
	- Install all needed project dependencies.
	```
	dotnet restore
	``` 

	- Build/compile the project.
	```
	dotnet build
	```

	- Run the compiled project.
	```
	dotnet run -p UcsdCapeScraper
	```

5. Follow the directions that the program provides.
</details>


## License
Please see the LICENSE file.