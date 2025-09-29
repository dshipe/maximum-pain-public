# maximum-pain-public
public repo for https://maximum-pain.com

Please request access to the private repo to contribute

Maximum-pain.com is a combination of SQL Server backend, .Net Core C# APIs and an Angular UI.  The UI is hosted on a AWS EC2 server.  The middle tier is a AWS lambda using a Function Url.  Function Urls are cheaper than API gateway.

Guthub is used for source control.  There is maximum-pain-com.CI.yaml that CI/CD the AWS Lambda code on merge

Option data is fetched from the Schwab APIs.  The data is cached in the  SQL Server database for 30 minutes.  This is just to keep from exceeding any limits Schwab may have.  

API Keys and passwords are maintained in AWS Secrets Manager and the code has a SecretService.cs wrapping it.



You can get the by using my REST APIs.  For example, there are the following APIs to return json data.  Please note I am using the stock symbol AAPL in these examples.  You can replace it with it any valid symbol

	option data:
	https://hcapr4ndhwksq5dq7ird3yujpq0edbbt.lambda-url.us-east-1.on.aws/api/options/straddle/aapl

I use abbreviated names in the JSON to reduce the size of the data.

	ot  = option symbol.  I use call symbol, knowing I need only replace C with P to get the put symbol
	d   = date (not applicable unless looking at my option history
	clp = call last price
	ca  = call ask 
	cb  = call bid
	coi = call open interest
	cv  = call volume
	civ = call implied volatility
	cde = call delta
	cga = call gamma
	cth = call theta
	cve = call vega
	crh = call rho
	plp = put last price
	pa  = put ask 
	pb  = put bid
	poi = put open interest
	pv  = put volume
	piv = put implied volatility
	pde = put delta
	pga = put gamma
	pth = put theta
	pve = put vega
	prh = put rho
	
Here is the API for max pain data

	maximum pain data
	http://maximum-pain.com/api/options/maxpain/aapl
	
	s   = strike
	coi = call open interest at this strike
	cch = call cash at this strike
	cpd = call percent difference
	poi = put open interest
	pch = put cash
	ppd = put percent difference	

The angular code takes the json data and renders an HTML table with Bootstrap CSS

For the the charts, I use Google Charts.  I have REST APIs that return json data for displaying the charts.  For example, the API to return the line for calls and puts as LineDoublePost

