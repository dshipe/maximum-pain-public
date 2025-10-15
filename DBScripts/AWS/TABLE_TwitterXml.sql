USE [MaxPainAPI]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='TwitterXml')
BEGIN
	DROP TABLE [TwitterXml]
END

CREATE TABLE [TwitterXml]
(
	Id bigint identity
	,Content text
	,ModifiedOn DateTime
)

INSERT INTO TwitterXml
(ModifiedOn, Content)
VALUES
(GetUTCDATE(),
'<Properties>
    <Property Name="LastPost" Value="12/25/2019 9:58:01 PM"/>
    <Property Name="LastRead" Value="12/25/2019 9:58:01 PM"/>
    <Property Name="LastMsg" Value="$CNP high OI range is 26.00 to 27.00 for option expiration 01/17/2020 PutCallRatio=0.20 #maxpain #options http://maximum-pain.com/options/CNP?m=01%2f17%2f2020"/>
    <Property Name="ConsumerKey" Value="cUQacuQKTGkGPiusBNUfXQ"/>
    <Property Name="ConsumerSecret" Value="p5gN3dBBJ8zDkbBhVwkc3NVlzcnALrOkvQnWWeRNJxc"/>
    <Property Name="NumberOfTweets" Value="6"/>
    <Property Name="NumberOfMinutes" Value="5"/>
    <Property Name="NumberOfTrending" Value="10"/>
    <Property Name="StockTwitsLastPost" Value="12/25/2019 6:12:46 PM"/>
    <Property Name="StockTwitsNumberOfMinutes" Value="30"/>
    <Property Name="TickerListDate" Value="12/22/2019 11:28:48 PM"/>
    <Property Name="TickerList" Value="#1,#1,#1,#1,#1,#2,#2,#2,#2,#2,#3,#3,#3,#3,#3,#4,#4,#4,#4,#4,#5,#5,#5,#5,#5,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,AAPL,AMZN,NFLX,FB,GOOG,TWTR,TSLA,PCLN,BAC,TSLA,LNKD,AMGN,QQQ,AAP,ABT,ADBE,ADI,ADP,AES,AGN,AIG,ALK,ALLE,ALXN,AMP,ANET,ANSS,AON,APA,APC,APD,ATO,ATVI,AVGO,BAX,BBT,BF.B,BIIB,BK,BWA,C,CAG,CAT,CBOE,CBRE,CCI,CCL,CDNS,CE,CF,CINF,CMI,CMS,CNC,COF,COG,COO,COP,CPRI,CSX,CTL,D,DAL,DHI,DISCA,DISH,DLTR,DRI,DTE,DVA,DVN,DXC,EBAY,ED,EIX,ES,ETFC,EVRG,EXC,EXPD,FAST,FBHS,FCX,FE,FFIV,FISV,FL,FLR,FLS,FLT,FOX,FRT,GE,GILD,GRMN,GWW,HAL,HBAN,HFC,HII,HRL,HRS,HST,HUM,IBM,IDXX,IFF,INCY,INTC,INTU,IPG,IR,ISRG,JBHT,JEC,JEF,JPM,JWN,K,KEY,KHC,KSS,LB,LEG,LKQ,LLL,LNC,LNT,LW,LYB,MA,MAR,MAS,MCD,MCHP,MGM,MHK,MKC,MLM,MMC,MMM,MNST,MO,MPC,MSCI,MTD,MU,NDAQ,NEM,NI,NKE,NKTR,NLSN,NOC,NRG,NTAP,NWL,NWS,NWSA,ORCL,ORLY,OXY,PCAR,PEG,PFE,PFG,PKI,PLD,PNC,PNR,PNW,PPG,PRU,PSX,PWR,PXD,QCOM,QRVO,RHT,RL,ROK,ROL,SCHW,SLB,SNA,SPGI,STX,STZ,TAP,TEL,TMK,TSS,UAL,UDR,ULTA,UNH,UNM,UPS,USB,UTX,V,VAR,VNO,VTR,WAB,WAT,WCG,WMT,WY,XEC,XRX,XYL,#2,#3,#3,#4,#4,#4,#5,SPX,SPX,SPX,SPX,AMZN,NFLX,FB,GOOG,TWTR,BAC,TSLA,LNKD,QQQ"/>"/>
    <Property Name="TickerListBackup" Value="#1,#1,#1,#1,#1,#2,#2,#2,#2,#2,#3,#3,#3,#3,#3,#4,#4,#4,#4,#4,#5,#5,#5,#5,#5,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,AAPL,AMZN,NFLX,FB,GOOG,TWTR,TSLA,PCLN,BAC,TSLA,LNKD,AMGN,QQQ,AAP,ABT,ADBE,ADI,ADP,AES,AGN,AIG,ALK,ALLE,ALXN,AMP,ANET,ANSS,AON,APA,APC,APD,ATO,ATVI,AVGO,BAX,BBT,BF.B,BIIB,BK,BWA,C,CAG,CAT,CBOE,CBRE,CCI,CCL,CDNS,CE,CF,CINF,CMI,CMS,CNC,COF,COG,COO,COP,CPRI,CSX,CTL,D,DAL,DHI,DISCA,DISH,DLTR,DRI,DTE,DVA,DVN,DXC,EBAY,ED,EIX,ES,ETFC,EVRG,EXC,EXPD,FAST,FBHS,FCX,FE,FFIV,FISV,FL,FLR,FLS,FLT,FOX,FRT,GE,GILD,GRMN,GWW,HAL,HBAN,HFC,HII,HRL,HRS,HST,HUM,IBM,IDXX,IFF,INCY,INTC,INTU,IPG,IR,ISRG,JBHT,JEC,JEF,JPM,JWN,K,KEY,KHC,KSS,LB,LEG,LKQ,LLL,LNC,LNT,LW,LYB,MA,MAR,MAS,MCD,MCHP,MGM,MHK,MKC,MLM,MMC,MMM,MNST,MO,MPC,MSCI,MTD,MU,NDAQ,NEM,NI,NKE,NKTR,NLSN,NOC,NRG,NTAP,NWL,NWS,NWSA,ORCL,ORLY,OXY,PCAR,PEG,PFE,PFG,PKI,PLD,PNC,PNR,PNW,PPG,PRU,PSX,PWR,PXD,QCOM,QRVO,RHT,RL,ROK,ROL,SCHW,SLB,SNA,SPGI,STX,STZ,TAP,TEL,TMK,TSS,UAL,UDR,ULTA,UNH,UNM,UPS,USB,UTX,V,VAR,VNO,VTR,WAB,WAT,WCG,WMT,WY,XEC,XRX,XYL,#2,#3,#3,#4,#4,#4,#5,SPX,SPX,SPX,SPX,AMZN,NFLX,FB,GOOG,TWTR,BAC,TSLA,LNKD,QQQ"/>"/>
    <!--  <Property Name="TickerListStatic" Value="$,AAPL,#1,AMZN,SPY,#2,NFLX,FB,#3,GOOG,$,TWTR,#4,PCLN,KNDI,#5,TSLA,LNKD,AMGN,QQQ,$" />  -->
    <Property Name="TickerListStatic" Value="#1,SPX,AAPL,AMZN,#2,SPX,NFLX,#3,SPX,FB,GOOG,#4,SPX,TWTR,TSLA,#5,SPX,PCLN,BAC,#1,SPX,TSLA,LNKD,#2,SPX,AMGN,QQQ,#3,SPX"/>
    <Property Name="TickerListSP500" Value="AAP,ABT,ADBE,ADI,ADP,AES,AGN,AIG,ALK,ALLE,ALXN,AMP,ANET,ANSS,AON,APA,APC,APD,ATO,ATVI,AVGO,BAX,BBT,BF.B,BIIB,BK,BWA,C,CAG,CAT,CBOE,CBRE,CCI,CCL,CDNS,CE,CF,CINF,CMI,CMS,CNC,COF,COG,COO,COP,CPRI,CSX,CTL,D,DAL,DHI,DISCA,DISH,DLTR,DRI,DTE,DVA,DVN,DXC,EBAY,ED,EIX,ES,ETFC,EVRG,EXC,EXPD,FAST,FBHS,FCX,FE,FFIV,FISV,FL,FLR,FLS,FLT,FOX,FRT,GE,GILD,GRMN,GWW,HAL,HBAN,HFC,HII,HRL,HRS,HST,HUM,IBM,IDXX,IFF,INCY,INTC,INTU,IPG,IR,ISRG,JBHT,JEC,JEF,JPM,JWN,K,KEY,KHC,KSS,LB,LEG,LKQ,LLL,LNC,LNT,LW,LYB,MA,MAR,MAS,MCD,MCHP,MGM,MHK,MKC,MLM,MMC,MMM,MNST,MO,MPC,MSCI,MTD,MU,NDAQ,NEM,NI,NKE,NKTR,NLSN,NOC,NRG,NTAP,NWL,NWS,NWSA,ORCL,ORLY,OXY,PCAR,PEG,PFE,PFG,PKI,PLD,PNC,PNR,PNW,PPG,PRU,PSX,PWR,PXD,QCOM,QRVO,RHT,RL,ROK,ROL,SCHW,SLB,SNA,SPGI,STX,STZ,TAP,TEL,TMK,TSS,UAL,UDR,ULTA,UNH,UNM,UPS,USB,UTX,V,VAR,VNO,VTR,WAB,WAT,WCG,WMT,WY,XEC,XRX,XYL,#2,#3,#3,#4,#4,#4,#5,SPX,SPX,SPX,SPX,AMZN,NFLX,FB,GOOG,TWTR,BAC,TSLA,LNKD,QQQ"/>
    <Property Name="TickerListSP500Backup" Value="A,AAL,AAP,AAPL,ABBV,ABC,ABMD,ABT,ACN,ADBE,ADI,ADM,ADP,ADS,ADSK,AEE,AEP,AES,AFL,AGN,AIG,AIV,AIZ,AJG,AKAM,ALB,ALGN,ALK,ALL,ALLE,ALXN,AMAT,AMD,AME,AMG,AMGN,AMP,AMT,AMZN,ANET,ANSS,ANTM,AON,AOS,APA,APC,APD,APH,APTV,ARE,ARNC,ATO,ATVI,AVB,AVGO,AVY,AWK,AXP,AZO,BA,BAC,BAX,BBT,BBY,BDX,BEN,BF.B,BHGE,BIIB,BK,BKNG,BLK,BLL,BMY,BR,BRK.B,BSX,BWA,BXP,C,CAG,CAH,CAT,CB,CBOE,CBRE,CBS,CCI,CCL,CDNS,CE,CELG,CERN,CF,CFG,CHD,CHRW,CHTR,CI,CINF,CL,CLX,CMA,CMCSA,CME,CMG,CMI,CMS,CNC,CNP,COF,COG,COO,COP,COST,COTY,CPB,CPRI,CPRT,CRM,CSCO,CSX,CTAS,CTL,CTSH,CTXS,CVS,CVX,CXO,D,DAL,DE,DFS,DG,DGX,DHI,DHR,DIS,DISCA,DISCK,DISH,DLR,DLTR,DOV,DOW,DRE,DRI,DTE,DUK,DVA,DVN,DXC,EA,EBAY,ECL,ED,EFX,EIX,EL,EMN,EMR,EOG,EQIX,EQR,ES,ESS,ETFC,ETN,ETR,EVRG,EW,EXC,EXPD,EXPE,EXR,F,FANG,FAST,FB,FBHS,FCX,FDX,FE,FFIV,FIS,FISV,FITB,FL,FLIR,FLR,FLS,FLT,FMC,FOX,FOXA,FRC,FRT,FTI,FTNT,FTV,GD,GE,GILD,GIS,GLW,GM,GOOG,GOOGL,GPC,GPN,GPS,GRMN,GS,GWW,HAL,HAS,HBAN,HBI,HCA,HCP,HD,HES,HFC,HIG,HII,HLT,HOG,HOLX,HON,HP,HPE,HPQ,HRB,HRL,HRS,HSIC,HST,HSY,HUM,IBM,ICE,IDXX,IFF,ILMN,INCY,INFO,INTC,INTU,IP,IPG,IPGP,IQV,IR,IRM,ISRG,IT,ITW,IVZ,JBHT,JCI,JEC,JEF,JKHY,JNJ,JNPR,JPM,JWN,K,KEY,KEYS,KHC,KIM,KLAC,KMB,KMI,KMX,KO,KR,KSS,KSU,L,LB,LEG,LEN,LH,LIN,LKQ,LLL,LLY,LMT,LNC,LNT,LOW,LRCX,LUV,LW,LYB,M,MA,MAA,MAC,MAR,MAS,MAT,MCD,MCHP,MCK,MCO,MDLZ,MDT,MET,MGM,MHK,MKC,MLM,MMC,MMM,MNST,MO,MOS,MPC,MRK,MRO,MS,MSCI,MSFT,MSI,MTB,MTD,MU,MXIM,MYL,NBL,NCLH,NDAQ,NEE,NEM,NFLX,NI,NKE,NKTR,NLSN,NOC,NOV,NRG,NSC,NTAP,NTRS,NUE,NVDA,NWL,NWS,NWSA,O,OKE,OMC,ORCL,ORLY,OXY,PAYX,PBCT,PCAR,PEG,PEP,PFE,PFG,PG,PGR,PH,PHM,PKG,PKI,PLD,PM,PNC,PNR,PNW,PPG,PPL,PRGO,PRU,PSA,PSX,PVH,PWR,PXD,PYPL,QCOM,QRVO,RCL,RE,REG,REGN,RF,RHI,RHT,RJF,RL,RMD,ROK,ROL,ROP,ROST,RSG,RTN,SBAC,SBUX,SCHW,SEE,SHW,SIVB,SJM,SLB,SLG,SNA,SNPS,SO,SPG,SPGI,SRE,STI,STT,STX,STZ,SWK,SWKS,SYF,SYK,SYMC,SYY,T,TAP,TDG,TEL,TFX,TGT,TIF,TJX,TMK,TMO,TPR,TRIP,TROW,TRV,TSCO,TSN,TSS,TTWO,TWTR,TXN,TXT,UA,UAA,UAL,UDR,UHS,ULTA,UNH,UNM,UNP,UPS,URI,USB,UTX,V,VAR,VFC,VIAB,VLO,VMC,VNO,VRSK,VRSN,VRTX,VTR,VZ,WAB,WAT,WBA,WCG,WDC,WEC,WELL,WFC,WHR,WLTW,WM,WMB,WMT,WRK,WU,WY,WYNN,XEC,XEL,XLNX,XOM,XRAY,XRX,XYL,YUM,ZBH,ZION,ZTS"/>
    <Property Name="TickerListSP500Static" Value="#1,#1,#1,#1,#1,#2,#2,#2,#2,#2,#3,#3,#3,#3,#3,#4,#4,#4,#4,#4,#5,#5,#5,#5,#5,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,SPX,AAPL,AMZN,NFLX,FB,GOOG,TWTR,TSLA,PCLN,BAC,TSLA,LNKD,AMGN,QQQ"/>
    <Property Name="InitialMsgNum" Value="1"/>
    <Property Name="UseImageTwitter" Value="false"/>
    <Property Name="UseImageStocktwits" Value="false"/>
    <Property Name="UseRetweet" Value="false"/>
    <Property Name="Status" Value="currentTime=12/25/2019 9:58:01 PM hour=21 sendTweets=True sendStockTwits=False DebugMode=False"/>
    <Hourly Date="12/25/2019 9:58:01 PM" Position="3" MsgNum="2"/>
    <Histories>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:00:52 AM" Position="3" MsgNum="2" Ticker="HSY"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:13:04 AM" Position="3" MsgNum="1" Ticker="BEN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:18:41 AM" Position="3" MsgNum="2" Ticker="NVDA"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:41:36 AM" Position="3" MsgNum="1" Ticker="#3"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:53:10 AM" Position="3" MsgNum="2" Ticker="ADS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:08:17 AM" Position="3" MsgNum="1" Ticker="#5"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:15:02 AM" Position="3" MsgNum="2" Ticker="ESS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:30:20 AM" Position="3" MsgNum="1" Ticker="SYF"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:39:24 AM" Position="3" MsgNum="2" Ticker="AMD"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:49:26 AM" Position="3" MsgNum="1" Ticker="DHR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:59:59 AM" Position="3" MsgNum="2" Ticker="#5"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:12:29 AM" Position="3" MsgNum="1" Ticker="ARE"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:27:29 AM" Position="3" MsgNum="2" Ticker="KR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:32:30 AM" Position="3" MsgNum="1" Ticker="#3"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:38:22 AM" Position="3" MsgNum="2" Ticker="GD"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:43:26 AM" Position="3" MsgNum="1" Ticker="GLW"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:48:48 AM" Position="3" MsgNum="2" Ticker="VIAB"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:53:51 AM" Position="3" MsgNum="1" Ticker="SBAC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:58:55 AM" Position="3" MsgNum="2" Ticker="FTV"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:13:58 AM" Position="3" MsgNum="1" Ticker="ANTM"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:19:39 AM" Position="3" MsgNum="2" Ticker="MAC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:25:39 AM" Position="3" MsgNum="1" Ticker="CPB"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:31:20 AM" Position="3" MsgNum="2" Ticker="SPX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:38:59 AM" Position="3" MsgNum="1" Ticker="IVZ"/>
        <History Type="WARNING" CurrentTime="12/23/2019 9:46:18 AM" Ticker="BRK.B" advanceReason="WARNING: No data for ticker BRK.B"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:46:18 AM" Position="3" MsgNum="2" Ticker="F"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:51:53 AM" Position="3" MsgNum="1" Ticker="EXR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:57:33 AM" Position="3" MsgNum="2" Ticker="PVH"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:03:12 AM" Position="3" MsgNum="1" Ticker="NUE"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:08:14 AM" Position="3" MsgNum="2" Ticker="MDT"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:14:31 AM" Position="3" MsgNum="1" Ticker="IRM"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:24:53 AM" Position="3" MsgNum="2" Ticker="HAS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:30:11 AM" Position="3" MsgNum="1" Ticker="RSG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:42:06 AM" Position="3" MsgNum="2" Ticker="RCL"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:47:16 AM" Position="3" MsgNum="1" Ticker="AIZ"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:52:32 AM" Position="3" MsgNum="2" Ticker="ADSK"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:59:52 AM" Position="3" MsgNum="1" Ticker="HP"/>
        <History Type="WARNING" CurrentTime="12/23/2019 11:05:45 AM" Ticker="PCLN" advanceReason="WARNING: No data for ticker PCLN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:05:45 AM" Position="3" MsgNum="2" Ticker="AJG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:12:07 AM" Position="3" MsgNum="1" Ticker="ARNC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:17:34 AM" Position="3" MsgNum="2" Ticker="DFS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:25:10 AM" Position="3" MsgNum="1" Ticker="SHW"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:38:36 AM" Position="3" MsgNum="2" Ticker="EQR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:44:16 AM" Position="3" MsgNum="1" Ticker="IPGP"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:00:16 PM" Position="3" MsgNum="2" Ticker="FMC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:05:59 PM" Position="3" MsgNum="1" Ticker="JNPR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:11:20 PM" Position="3" MsgNum="2" Ticker="HCP"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:17:20 PM" Position="3" MsgNum="1" Ticker="VFC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:22:43 PM" Position="3" MsgNum="2" Ticker="CSCO"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:28:31 PM" Position="3" MsgNum="1" Ticker="DE"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:33:33 PM" Position="3" MsgNum="2" Ticker="XEL"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:38:36 PM" Position="3" MsgNum="1" Ticker="TSLA"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:44:28 PM" Position="3" MsgNum="2" Ticker="EL"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:50:17 PM" Position="3" MsgNum="1" Ticker="BHGE"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 12:55:54 PM" Position="3" MsgNum="2" Ticker="AAPL"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:04:56 PM" Position="3" MsgNum="1" Ticker="WDC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:10:08 PM" Position="3" MsgNum="2" Ticker="HOG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:15:22 PM" Position="3" MsgNum="1" Ticker="DG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:21:51 PM" Position="3" MsgNum="2" Ticker="#5"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:26:54 PM" Position="3" MsgNum="1" Ticker="#5"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:35:07 PM" Position="3" MsgNum="2" Ticker="KMB"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:42:04 PM" Position="3" MsgNum="1" Ticker="GOOG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:51:50 PM" Position="3" MsgNum="2" Ticker="SLG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 1:58:46 PM" Position="3" MsgNum="1" Ticker="JKHY"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:04:32 PM" Position="3" MsgNum="2" Ticker="COST"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:14:33 PM" Position="3" MsgNum="1" Ticker="CMG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:21:10 PM" Position="3" MsgNum="2" Ticker="AKAM"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:27:16 PM" Position="3" MsgNum="1" Ticker="SWK"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:32:31 PM" Position="3" MsgNum="2" Ticker="CLX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:39:37 PM" Position="3" MsgNum="1" Ticker="CTSH"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:46:45 PM" Position="3" MsgNum="2" Ticker="CELG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 2:54:20 PM" Position="3" MsgNum="1" Ticker="NSC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:04:19 PM" Position="3" MsgNum="2" Ticker="FIS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:09:20 PM" Position="3" MsgNum="1" Ticker="ITW"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:14:27 PM" Position="3" MsgNum="2" Ticker="CERN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:23:46 PM" Position="3" MsgNum="1" Ticker="AMG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:29:22 PM" Position="3" MsgNum="2" Ticker="JCI"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:35:01 PM" Position="3" MsgNum="1" Ticker="FOXA"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:40:11 PM" Position="3" MsgNum="2" Ticker="PSA"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:46:05 PM" Position="3" MsgNum="1" Ticker="#4"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 3:53:12 PM" Position="3" MsgNum="2" Ticker="JNJ"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:00:40 PM" Position="3" MsgNum="1" Ticker="XLNX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:09:56 PM" Position="3" MsgNum="2" Ticker="DOV"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:18:15 PM" Position="3" MsgNum="1" Ticker="SYK"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:24:19 PM" Position="3" MsgNum="2" Ticker="SPX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:29:48 PM" Position="3" MsgNum="1" Ticker="CHTR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:43:16 PM" Position="3" MsgNum="2" Ticker="TTWO"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:52:21 PM" Position="3" MsgNum="1" Ticker="HLT"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 4:57:22 PM" Position="3" MsgNum="2" Ticker="MSI"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 5:02:54 PM" Position="3" MsgNum="1" Ticker="PM"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 5:08:20 PM" Position="3" MsgNum="2" Ticker="GIS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 5:13:22 PM" Position="3" MsgNum="1" Ticker="NFLX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 5:19:15 PM" Position="3" MsgNum="2" Ticker="ETN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 5:25:33 PM" Position="3" MsgNum="1" Ticker="BKNG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 5:33:52 PM" Position="3" MsgNum="2" Ticker="SIVB"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 5:50:31 PM" Position="3" MsgNum="1" Ticker="ICE"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:04:05 PM" Position="3" MsgNum="2" Ticker="IQV"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:13:50 PM" Position="3" MsgNum="1" Ticker="CHRW"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:21:57 PM" Position="3" MsgNum="2" Ticker="CB"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:30:03 PM" Position="3" MsgNum="1" Ticker="CAH"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:39:13 PM" Position="3" MsgNum="2" Ticker="TJX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:48:38 PM" Position="3" MsgNum="1" Ticker="PRGO"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 6:54:05 PM" Position="3" MsgNum="2" Ticker="PBCT"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:00:40 PM" Position="3" MsgNum="1" Ticker="VMC"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:06:02 PM" Position="3" MsgNum="2" Ticker="CTXS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:14:18 PM" Position="3" MsgNum="1" Ticker="CMCSA"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:19:23 PM" Position="3" MsgNum="2" Ticker="CMA"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:31:22 PM" Position="3" MsgNum="1" Ticker="#4"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:36:50 PM" Position="3" MsgNum="2" Ticker="WHR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 7:45:23 PM" Position="3" MsgNum="1" Ticker="MAT"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:20:14 PM" Position="3" MsgNum="2" Ticker="L"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:32:10 PM" Position="3" MsgNum="1" Ticker="ALB"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:45:57 PM" Position="3" MsgNum="2" Ticker="BR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 8:52:18 PM" Position="3" MsgNum="1" Ticker="AMT"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:04:41 PM" Position="3" MsgNum="2" Ticker="XOM"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:17:59 PM" Position="3" MsgNum="1" Ticker="ABBV"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:24:56 PM" Position="3" MsgNum="2" Ticker="KO"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:31:22 PM" Position="3" MsgNum="1" Ticker="TIF"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:39:24 PM" Position="3" MsgNum="2" Ticker="LOW"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:45:17 PM" Position="3" MsgNum="1" Ticker="HPQ"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 9:56:00 PM" Position="3" MsgNum="2" Ticker="MTB"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:06:13 PM" Position="3" MsgNum="1" Ticker="TROW"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:24:59 PM" Position="3" MsgNum="2" Ticker="RTN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:30:31 PM" Position="3" MsgNum="1" Ticker="PGR"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:35:43 PM" Position="3" MsgNum="2" Ticker="EMN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 10:41:55 PM" Position="3" MsgNum="1" Ticker="MDLZ"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:01:12 PM" Position="3" MsgNum="2" Ticker="CVX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:11:26 PM" Position="3" MsgNum="1" Ticker="DIS"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:20:10 PM" Position="3" MsgNum="2" Ticker="EQIX"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:26:33 PM" Position="3" MsgNum="1" Ticker="MSFT"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:37:59 PM" Position="3" MsgNum="2" Ticker="AMGN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:46:38 PM" Position="3" MsgNum="1" Ticker="REGN"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:52:52 PM" Position="3" MsgNum="2" Ticker="CFG"/>
        <History Type="HourlyPost" CurrentTime="12/23/2019 11:58:30 PM" Position="3" MsgNum="1" Ticker="MXIM"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 6:09:15 AM" Position="3" MsgNum="2" Ticker="SYY"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 6:28:33 AM" Position="3" MsgNum="1" Ticker="RMD"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 6:37:50 AM" Position="3" MsgNum="2" Ticker="AAPL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:07:04 AM" Position="3" MsgNum="1" Ticker="CVS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:12:30 AM" Position="3" MsgNum="2" Ticker="CHD"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:23:42 AM" Position="3" MsgNum="1" Ticker="WU"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:41:12 AM" Position="3" MsgNum="2" Ticker="KMI"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:46:17 AM" Position="3" MsgNum="1" Ticker="AMGN"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:53:35 AM" Position="3" MsgNum="2" Ticker="AOS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:02:08 AM" Position="3" MsgNum="1" Ticker="TGT"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:11:46 AM" Position="3" MsgNum="2" Ticker="BSX"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:20:39 AM" Position="3" MsgNum="1" Ticker="AWK"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:28:09 AM" Position="3" MsgNum="2" Ticker="UAA"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:33:13 AM" Position="3" MsgNum="1" Ticker="LRCX"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:43:31 AM" Position="3" MsgNum="2" Ticker="REG"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:50:57 AM" Position="3" MsgNum="1" Ticker="ECL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:00:29 AM" Position="3" MsgNum="2" Ticker="EMR"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:10:33 AM" Position="3" MsgNum="1" Ticker="HBI"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:18:46 AM" Position="3" MsgNum="2" Ticker="AEE"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:26:00 AM" Position="3" MsgNum="1" Ticker="EOG"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:31:18 AM" Position="3" MsgNum="2" Ticker="GM"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:36:33 AM" Position="3" MsgNum="1" Ticker="MCO"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:48:09 AM" Position="3" MsgNum="2" Ticker="XRAY"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:55:10 AM" Position="3" MsgNum="1" Ticker="MS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:00:11 AM" Position="3" MsgNum="2" Ticker="EW"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:05:23 AM" Position="3" MsgNum="1" Ticker="SJM"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:12:54 AM" Position="3" MsgNum="2" Ticker="A"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:26:51 AM" Position="3" MsgNum="1" Ticker="BLK"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:33:26 AM" Position="3" MsgNum="2" Ticker="CL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:46:37 AM" Position="3" MsgNum="1" Ticker="VRSN"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:52:23 AM" Position="3" MsgNum="2" Ticker="SNPS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:58:58 AM" Position="3" MsgNum="1" Ticker="AZO"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:04:55 AM" Position="3" MsgNum="2" Ticker="LLY"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:11:28 AM" Position="3" MsgNum="1" Ticker="AXP"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:16:53 AM" Position="3" MsgNum="2" Ticker="PYPL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:26:30 AM" Position="3" MsgNum="1" Ticker="WFC"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:36:45 AM" Position="3" MsgNum="2" Ticker="DISCK"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:43:17 AM" Position="3" MsgNum="1" Ticker="AIV"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 12:00:59 PM" Position="3" MsgNum="2" Ticker="TDG"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 12:06:49 PM" Position="3" MsgNum="1" Ticker="GPS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 12:16:55 PM" Position="3" MsgNum="2" Ticker="KEYS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 12:23:07 PM" Position="3" MsgNum="1" Ticker="WLTW"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 12:28:53 PM" Position="3" MsgNum="2" Ticker="RHI"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 12:49:52 PM" Position="3" MsgNum="1" Ticker="MYL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 12:56:56 PM" Position="3" MsgNum="2" Ticker="NBL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:03:54 PM" Position="3" MsgNum="1" Ticker="HRB"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:08:54 PM" Position="3" MsgNum="2" Ticker="FLIR"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:16:33 PM" Position="3" MsgNum="1" Ticker="AFL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:22:05 PM" Position="3" MsgNum="2" Ticker="TWTR"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:27:12 PM" Position="3" MsgNum="1" Ticker="LEN"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:33:08 PM" Position="3" MsgNum="2" Ticker="SBUX"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:40:17 PM" Position="3" MsgNum="1" Ticker="MRO"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:45:32 PM" Position="3" MsgNum="2" Ticker="UHS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:51:05 PM" Position="3" MsgNum="1" Ticker="PAYX"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 1:56:40 PM" Position="3" MsgNum="2" Ticker="KIM"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:02:50 PM" Position="3" MsgNum="1" Ticker="#2"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:11:15 PM" Position="3" MsgNum="2" Ticker="SRE"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:16:50 PM" Position="3" MsgNum="1" Ticker="DOW"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:22:21 PM" Position="3" MsgNum="2" Ticker="HPE"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:28:00 PM" Position="3" MsgNum="1" Ticker="#1"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:30:34 PM" Position="3" MsgNum="2" Ticker="IT"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:37:03 PM" Position="3" MsgNum="1" Ticker="WRK"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:46:49 PM" Position="3" MsgNum="2" Ticker="VRTX"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:53:05 PM" Position="3" MsgNum="1" Ticker="GPC"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 2:58:06 PM" Position="3" MsgNum="2" Ticker="CXO"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 3:06:53 PM" Position="3" MsgNum="1" Ticker="AVB"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 3:26:38 PM" Position="3" MsgNum="2" Ticker="PG"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 3:55:56 PM" Position="3" MsgNum="1" Ticker="HIG"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 4:13:24 PM" Position="3" MsgNum="2" Ticker="INFO"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 4:20:54 PM" Position="3" MsgNum="1" Ticker="AMZN"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 4:30:41 PM" Position="3" MsgNum="2" Ticker="BAC"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 4:40:32 PM" Position="3" MsgNum="1" Ticker="EXPE"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 5:06:58 PM" Position="3" MsgNum="2" Ticker="FITB"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 5:33:25 PM" Position="3" MsgNum="1" Ticker="VLO"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 6:34:49 PM" Position="3" MsgNum="2" Ticker="TSCO"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 6:40:54 PM" Position="3" MsgNum="1" Ticker="STI"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 6:46:37 PM" Position="3" MsgNum="2" Ticker="LH"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 6:56:12 PM" Position="3" MsgNum="1" Ticker="MCK"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:29:26 PM" Position="3" MsgNum="2" Ticker="NCLH"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 7:56:54 PM" Position="3" MsgNum="1" Ticker="DUK"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:02:16 PM" Position="3" MsgNum="2" Ticker="YUM"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:07:45 PM" Position="3" MsgNum="1" Ticker="LUV"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:15:49 PM" Position="3" MsgNum="2" Ticker="SPX"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:20:49 PM" Position="3" MsgNum="1" Ticker="MRK"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:42:52 PM" Position="3" MsgNum="2" Ticker="KLAC"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 8:47:53 PM" Position="3" MsgNum="1" Ticker="AAL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:01:44 PM" Position="3" MsgNum="2" Ticker="SPG"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:08:03 PM" Position="3" MsgNum="1" Ticker="FB"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:16:08 PM" Position="3" MsgNum="2" Ticker="NOV"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 9:36:19 PM" Position="3" MsgNum="1" Ticker="BLL"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:19:26 PM" Position="3" MsgNum="2" Ticker="HES"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:43:53 PM" Position="3" MsgNum="1" Ticker="WM"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:53:00 PM" Position="3" MsgNum="2" Ticker="#2"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 10:58:59 PM" Position="3" MsgNum="1" Ticker="FTNT"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:08:13 PM" Position="3" MsgNum="2" Ticker="TXN"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:14:39 PM" Position="3" MsgNum="1" Ticker="ROST"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:23:17 PM" Position="3" MsgNum="2" Ticker="AME"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:36:53 PM" Position="3" MsgNum="1" Ticker="SWKS"/>
        <History Type="HourlyPost" CurrentTime="12/24/2019 11:51:24 PM" Position="3" MsgNum="2" Ticker="BA"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:03:44 AM" Position="3" MsgNum="1" Ticker="ZBH"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:21:23 AM" Position="3" MsgNum="2" Ticker="BDX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:27:22 AM" Position="3" MsgNum="1" Ticker="#2"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:39:26 AM" Position="3" MsgNum="2" Ticker="CRM"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:51:13 AM" Position="3" MsgNum="1" Ticker="STT"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:57:33 AM" Position="3" MsgNum="2" Ticker="ILMN"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:06:43 AM" Position="3" MsgNum="1" Ticker="OKE"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:14:11 AM" Position="3" MsgNum="2" Ticker="SYMC"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:20:13 AM" Position="3" MsgNum="1" Ticker="WEC"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:27:10 AM" Position="3" MsgNum="2" Ticker="SPX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:32:52 AM" Position="3" MsgNum="1" Ticker="TFX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:37:58 AM" Position="3" MsgNum="2" Ticker="BXP"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:43:31 AM" Position="3" MsgNum="1" Ticker="ROP"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:55:29 AM" Position="3" MsgNum="2" Ticker="CTAS"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:35:33 AM" Position="3" MsgNum="1" Ticker="GPN"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:42:22 AM" Position="3" MsgNum="2" Ticker="RE"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:03:33 AM" Position="3" MsgNum="1" Ticker="CI"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:15:55 AM" Position="3" MsgNum="2" Ticker="#1"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:21:28 AM" Position="3" MsgNum="1" Ticker="CBS"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:27:07 AM" Position="3" MsgNum="2" Ticker="VRSK"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:35:52 AM" Position="3" MsgNum="1" Ticker="WELL"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:55:52 AM" Position="3" MsgNum="2" Ticker="O"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:03:33 AM" Position="3" MsgNum="1" Ticker="MOS"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:08:37 AM" Position="3" MsgNum="2" Ticker="FTI"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:15:15 AM" Position="3" MsgNum="1" Ticker="PHM"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:23:38 AM" Position="3" MsgNum="2" Ticker="ALL"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:29:22 AM" Position="3" MsgNum="1" Ticker="ACN"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:34:33 AM" Position="3" MsgNum="2" Ticker="#1"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:34:41 AM" Position="3" MsgNum="1" Ticker="IP"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:46:53 AM" Position="3" MsgNum="2" Ticker="#1"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:47:12 AM" Position="3" MsgNum="1" Ticker="SPX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:53:59 AM" Position="3" MsgNum="2" Ticker="BBY"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 10:59:55 AM" Position="3" MsgNum="1" Ticker="SEE"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 11:10:10 AM" Position="3" MsgNum="2" Ticker="RF"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 11:26:11 AM" Position="3" MsgNum="1" Ticker="#2"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 11:59:35 AM" Position="3" MsgNum="2" Ticker="OMC"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 12:10:03 PM" Position="3" MsgNum="1" Ticker="M"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 12:24:18 PM" Position="3" MsgNum="2" Ticker="HSIC"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 12:34:37 PM" Position="3" MsgNum="1" Ticker="BMY"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 12:41:19 PM" Position="3" MsgNum="2" Ticker="GS"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 12:47:20 PM" Position="3" MsgNum="1" Ticker="DGX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 1:03:13 PM" Position="3" MsgNum="2" Ticker="KSU"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 1:12:04 PM" Position="3" MsgNum="1" Ticker="ZION"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 1:18:36 PM" Position="3" MsgNum="2" Ticker="NEE"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 1:24:36 PM" Position="3" MsgNum="1" Ticker="CME"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 1:34:12 PM" Position="3" MsgNum="2" Ticker="ABC"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 1:43:07 PM" Position="3" MsgNum="1" Ticker="GOOGL"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 1:53:33 PM" Position="3" MsgNum="2" Ticker="ABMD"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 2:00:11 PM" Position="3" MsgNum="1" Ticker="KMX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 2:17:46 PM" Position="3" MsgNum="2" Ticker="#1"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 2:21:57 PM" Position="3" MsgNum="1" Ticker="HOLX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 2:35:24 PM" Position="3" MsgNum="2" Ticker="SPX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 2:40:43 PM" Position="3" MsgNum="1" Ticker="HCA"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 2:47:31 PM" Position="3" MsgNum="2" Ticker="HD"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 2:56:08 PM" Position="3" MsgNum="1" Ticker="COTY"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 3:12:00 PM" Position="3" MsgNum="2" Ticker="NTRS"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 3:30:21 PM" Position="3" MsgNum="1" Ticker="ALGN"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 3:43:42 PM" Position="3" MsgNum="2" Ticker="TPR"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 3:58:46 PM" Position="3" MsgNum="1" Ticker="ADM"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:06:25 PM" Position="3" MsgNum="2" Ticker="T"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:12:13 PM" Position="3" MsgNum="1" Ticker="TRV"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:19:29 PM" Position="3" MsgNum="2" Ticker="PEP"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:25:54 PM" Position="3" MsgNum="1" Ticker="EFX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:31:56 PM" Position="3" MsgNum="2" Ticker="ETR"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:40:08 PM" Position="3" MsgNum="1" Ticker="LMT"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:45:11 PM" Position="3" MsgNum="2" Ticker="UNP"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 4:52:14 PM" Position="3" MsgNum="1" Ticker="MAA"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 5:00:44 PM" Position="3" MsgNum="2" Ticker="MET"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 5:05:48 PM" Position="3" MsgNum="1" Ticker="TXT"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 5:11:12 PM" Position="3" MsgNum="2" Ticker="SO"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 5:16:42 PM" Position="3" MsgNum="1" Ticker="TMO"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 5:26:55 PM" Position="3" MsgNum="2" Ticker="APH"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 5:38:47 PM" Position="3" MsgNum="1" Ticker="VZ"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:12:46 PM" Position="3" MsgNum="2" Ticker="AVY"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:23:14 PM" Position="3" MsgNum="1" Ticker="DLR"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:33:06 PM" Position="3" MsgNum="2" Ticker="TSN"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 6:38:13 PM" Position="3" MsgNum="1" Ticker="WMB"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:01:13 PM" Position="3" MsgNum="2" Ticker="FANG"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:15:49 PM" Position="3" MsgNum="1" Ticker="PH"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:27:27 PM" Position="3" MsgNum="2" Ticker="LIN"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:43:42 PM" Position="3" MsgNum="1" Ticker="UA"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 7:52:50 PM" Position="3" MsgNum="2" Ticker="AMAT"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:19:07 PM" Position="3" MsgNum="1" Ticker="AEP"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:26:19 PM" Position="3" MsgNum="2" Ticker="EA"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:31:41 PM" Position="3" MsgNum="1" Ticker="PKG"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:37:56 PM" Position="3" MsgNum="2" Ticker="URI"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:43:22 PM" Position="3" MsgNum="1" Ticker="APTV"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:50:09 PM" Position="3" MsgNum="2" Ticker="DRE"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 8:56:47 PM" Position="3" MsgNum="1" Ticker="FDX"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:04:42 PM" Position="3" MsgNum="2" Ticker="RJF"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:12:18 PM" Position="3" MsgNum="1" Ticker="ZTS"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:18:00 PM" Position="3" MsgNum="2" Ticker="WYNN"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:25:13 PM" Position="3" MsgNum="1" Ticker="WBA"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:40:09 PM" Position="3" MsgNum="2" Ticker="HON"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:48:32 PM" Position="3" MsgNum="1" Ticker="CPRT"/>
        <History Type="HourlyPost" CurrentTime="12/25/2019 9:58:01 PM" Position="3" MsgNum="2" Ticker="CNP"/>
    </Histories>
    <Retweets>
        <Retweet ID="1017177501498531842" Date="7/12/2018 11:31:00 AM"/>
        <Retweet ID="1017370495556243456" Date="7/12/2018 11:41:26 AM"/>
        <Retweet ID="1017376830716874759" Date="7/12/2018 11:58:39 AM"/>
        <Retweet ID="1017373264786620416" Date="7/12/2018 11:58:39 AM"/>
        <Retweet ID="1017376944739045376" Date="7/12/2018 1:05:32 PM"/>
        <Retweet ID="1017393438856269824" Date="7/12/2018 1:42:50 PM"/>
        <Retweet ID="1017403819917901825" Date="7/12/2018 2:50:19 PM"/>
        <Retweet ID="1017420733646098434" Date="7/12/2018 3:56:26 PM"/>
        <Retweet ID="1017437260151746560" Date="7/12/2018 8:01:18 PM"/>
        <Retweet ID="1017498215904628736" Date="7/12/2018 10:01:27 PM"/>
        <Retweet ID="1017529040641589248" Date="7/13/2018 12:05:47 PM"/>
        <Retweet ID="1017741441714589696" Date="7/13/2018 12:15:50 PM"/>
        <Retweet ID="1017743957642694658" Date="7/13/2018 1:57:51 PM"/>
        <Retweet ID="1017768829089435654" Date="7/13/2018 5:52:12 PM"/>
        <Retweet ID="1017828434888876032" Date="7/13/2018 7:44:20 PM"/>
        <Retweet ID="1017856096399183872" Date="7/13/2018 9:38:33 PM"/>
        <Retweet ID="1017884830422446080" Date="7/13/2018 10:04:02 PM"/>
        <Retweet ID="1017891427638968322" Date="7/13/2018 10:19:42 PM"/>
        <Retweet ID="1017895724464443393" Date="7/14/2018 10:05:52 PM"/>
        <Retweet ID="1018253817484210176" Date="7/16/2018 2:06:13 PM"/>
        <Retweet ID="1018616203445710849" Date="7/16/2018 2:26:46 PM"/>
        <Retweet ID="1018865596040179716" Date="7/16/2018 2:31:48 PM"/>
        <Retweet ID="1018864470758690816" Date="7/16/2018 2:31:48 PM"/>
        <Retweet ID="1018865764227375105" Date="7/16/2018 2:36:49 PM"/>
        <Retweet ID="1018923412876603392" Date="7/16/2018 6:21:48 PM"/>
        <Retweet ID="1018865900483575808" Date="7/16/2018 6:21:49 PM"/>
        <Retweet ID="1018923507223224320" Date="7/16/2018 9:29:26 PM"/>
        <Retweet ID="1018969507266682880" Date="7/16/2018 10:01:26 PM"/>
        <Retweet ID="1018978590598352897" Date="7/17/2018 12:21:09 PM"/>
        <Retweet ID="1019196778023878656" Date="7/17/2018 12:32:11 PM"/>
        <Retweet ID="1019194262544674816" Date="7/17/2018 12:32:12 PM"/>
        <Retweet ID="1019196801910333440" Date="7/17/2018 1:34:40 PM"/>
        <Retweet ID="1019213639654498305" Date="7/17/2018 2:42:17 PM"/>
        <Retweet ID="1019229788169523203" Date="7/17/2018 4:15:24 PM"/>
        <Retweet ID="1019253693693038595" Date="7/17/2018 6:03:39 PM"/>
        <Retweet ID="1019280461325975552" Date="7/17/2018 6:39:21 PM"/>
        <Retweet ID="1019356078721720331" Date="7/17/2018 11:01:44 PM"/>
        <Retweet ID="1019289549455806465" Date="7/17/2018 11:01:45 PM"/>
        <Retweet ID="1019356245688369152" Date="7/17/2018 11:11:53 PM"/>
        <Retweet ID="1019358091983376384" Date="7/18/2018 1:22:31 PM"/>
        <Retweet ID="1019572758772998150" Date="7/18/2018 1:27:28 PM"/>
        <Retweet ID="1019573895966412800" Date="7/18/2018 1:38:00 PM"/>
        <Retweet ID="1019576111183917056" Date="7/18/2018 1:43:04 PM"/>
        <Retweet ID="1019577914894012416" Date="7/18/2018 5:02:50 PM"/>
        <Retweet ID="1019627985941323776" Date="7/18/2018 5:33:52 PM"/>
        <Retweet ID="1019635271858556929" Date="7/18/2018 10:02:05 PM"/>
        <Retweet ID="1019703369194262530" Date="7/19/2018 12:27:19 AM"/>
        <Retweet ID="1019742586498453504" Date="7/19/2018 12:37:28 AM"/>
        <Retweet ID="1019739353701117952" Date="7/19/2018 12:37:28 AM"/>
    </Retweets>
    <TweetList xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <Tweets>
            <Tweet>
                <ID>1210029608021696512</ID>
                <Created_At>Thu Dec 26 02:48:34 +0000 2019</Created_At>
                <Source>&lt;a href="http://maximum-pain.com" rel="nofollow"&gt;maxpain &lt;/a &gt;</Source>
                <Full_Text>$CPRT Max Pain=85.00. Maturity=01/17/2020. #maxpain #options https://t.co/hHBGctoHqd https://t.co/JwVu0AQJiy</Full_Text>
                <Retweet_Count>0</Retweet_Count>
                <User>
                    <ID>716737614771044352</ID>
                    <Name>max pain</Name>
                    <Screen_Name>optioncharts</Screen_Name>
                    <Location/>
                    <Description/>
                    <Url>https://t.co/eIsTCt8fCQ</Url>
                    <Followers_Count>1556</Followers_Count>
                    <Friends_Count>10</Friends_Count>
                    <Listed_Count>55</Listed_Count>
                </User>
            </Tweet>
            <Tweet>
                <ID>1210027504263712768</ID>
                <Created_At>Thu Dec 26 02:40:12 +0000 2019</Created_At>
                <Source>&lt;a href="http://maximum-pain.com" rel="nofollow"&gt;maxpain &lt;/a &gt;</Source>
                <Full_Text>$HON high OI range is 177.50 to 177.50 for option expiration 12/27/2019 PutCallRatio=0.91 #maxpain #options https://t.co/sKjEuTL4Dg https://t.co/sXJO9Y2Jrq</Full_Text>
                <Retweet_Count>0</Retweet_Count>
                <User>
                    <ID>716737614771044352</ID>
                    <Name>max pain</Name>
                    <Screen_Name>optioncharts</Screen_Name>
                    <Location/>
                    <Description/>
                    <Url>https://t.co/eIsTCt8fCQ</Url>
                    <Followers_Count>1556</Followers_Count>
                    <Friends_Count>10</Friends_Count>
                    <Listed_Count>55</Listed_Count>
                </User>
            </Tweet>
            <Tweet>
                <ID>1210023749656952833</ID>
                <Created_At>Thu Dec 26 02:25:17 +0000 2019</Created_At>
                <Source>&lt;a href="http://maximum-pain.com" rel="nofollow"&gt;maxpain &lt;/a &gt;</Source>
                <Full_Text>$WBA Max Pain is 58.00 for maturity 12/27/2019. #maxpain #options https://t.co/jtP3EsD5Nh https://t.co/2XEvhgANZa</Full_Text>
                <Retweet_Count>0</Retweet_Count>
                <User>
                    <ID>716737614771044352</ID>
                    <Name>max pain</Name>
                    <Screen_Name>optioncharts</Screen_Name>
                    <Location/>
                    <Description/>
                    <Url>https://t.co/eIsTCt8fCQ</Url>
                    <Followers_Count>1556</Followers_Count>
                    <Friends_Count>10</Friends_Count>
                    <Listed_Count>55</Listed_Count>
                </User>
            </Tweet>
            <Tweet>
                <ID>1210021929740120064</ID>
                <Created_At>Thu Dec 26 02:18:03 +0000 2019</Created_At>
                <Source>&lt;a href="http://maximum-pain.com" rel="nofollow"&gt;maxpain &lt;/a &gt;</Source>
                <Full_Text>$WYNN open interest for maturity 12/27/2019. High put=141.00 High call=142.00 PutCallRatio=0.60 #maxpain #options https://t.co/PrxhKxTGnJ https://t.co/Dd17PaOwAx</Full_Text>
                <Retweet_Count>0</Retweet_Count>
                <User>
                    <ID>716737614771044352</ID>
                    <Name>max pain</Name>
                    <Screen_Name>optioncharts</Screen_Name>
                    <Location/>
                    <Description/>
                    <Url>https://t.co/eIsTCt8fCQ</Url>
                    <Followers_Count>1556</Followers_Count>
                    <Friends_Count>10</Friends_Count>
                    <Listed_Count>55</Listed_Count>
                </User>
            </Tweet>
            <Tweet>
                <ID>1210020492024930307</ID>
                <Created_At>Thu Dec 26 02:12:21 +0000 2019</Created_At>
                <Source>&lt;a href="http://maximum-pain.com" rel="nofollow"&gt;maxpain &lt;/a &gt;</Source>
                <Full_Text>$ZTS Max Pain=124.00. Maturity=12/27/2019. #maxpain #options https://t.co/IiNbt3payR https://t.co/PI27OOWWlX</Full_Text>
                <Retweet_Count>0</Retweet_Count>
                <User>
                    <ID>716737614771044352</ID>
                    <Name>max pain</Name>
                    <Screen_Name>optioncharts</Screen_Name>
                    <Location/>
                    <Description/>
                    <Url>https://t.co/eIsTCt8fCQ</Url>
                    <Followers_Count>1556</Followers_Count>
                    <Friends_Count>10</Friends_Count>
                    <Listed_Count>55</Listed_Count>
                </User>
            </Tweet>
            <Tweet>
                <ID>1210018576368914432</ID>
                <Created_At>Thu Dec 26 02:04:44 +0000 2019</Created_At>
                <Source>&lt;a href="http://maximum-pain.com" rel="nofollow"&gt;maxpain &lt;/a &gt;</Source>
                <Full_Text>$RJF open interest for maturity 01/17/2020. High put=87.50 High call=92.50 PutCallRatio=1.43 #maxpain #options https://t.co/muD6vhKiNR https://t.co/d4cDadPbNF</Full_Text>
                <Retweet_Count>0</Retweet_Count>
                <User>
                    <ID>716737614771044352</ID>
                    <Name>max pain</Name>
                    <Screen_Name>optioncharts</Screen_Name>
                    <Location/>
                    <Description/>
                    <Url>https://t.co/eIsTCt8fCQ</Url>
                    <Followers_Count>1556</Followers_Count>
                    <Friends_Count>10</Friends_Count>
                    <Listed_Count>55</Listed_Count>
                </User>
            </Tweet>
        </Tweets>
    </TweetList>
</Properties>'

)

SELECT Convert(xml,Content) FROM TwitterXml