from bs4 import BeautifulSoup
import requests
import operator
import re
import time
import json
import pandas as pd


baseUrl = 'https://www.rightmove.co.uk'
htmlparser = 'html.parser'
lxml = 'lxml'
index = 0


## Methods ##
def GetPropertyUrls():
    try:
        #url = input("Enter RightMove Url : ")
        url = "https://www.rightmove.co.uk/property-for-sale/find.html?searchType=SALE&locationIdentifier=REGION%5E3357&insId=1&radius=0.0&minPrice=&maxPrice=&minBedrooms=&maxBedrooms=&displayPropertyType=&maxDaysSinceAdded=&_includeSSTC=on&sortByPriceDescending=&primaryDisplayPropertyType=&secondaryDisplayPropertyType=&oldDisplayPropertyType=&oldPrimaryDisplayPropertyType=&newHome=&auction=false"
        
        print("Fatching data form " + url)
        
        response = requests.get(url)
        soup = BeautifulSoup(response.text, htmlparser)
        return [item.attrs.get('href') for item in soup.select('div.propertyCard-wrapper a')]
    except Exception as ex:
        print(ex.args[0])
        return[]


## Main Part starts from here ##
try:
    propertyUrls = GetPropertyUrls()

    if(propertyUrls):
        for _url in propertyUrls:
            if(_url != 'None'):
                
                index+=1
                print(str(index) + ") Fatching data form " + baseUrl + _url)
                
                responsePropertyDetailPage = requests.get(baseUrl + _url)
                propertyDetailPage = BeautifulSoup(responsePropertyDetailPage.text,htmlparser)
                data = propertyDetailPage

                df= pd.DataFrame(data)
                
                with pd.option_context('display.max_rows', None, 'display.max_columns', df.shape[1]):
                    print(df)

except requests.ConnectionError as exc:
    # filtering DNS lookup error from other connection errors
    # (until https://github.com/shazow/urllib3/issues/1003 is resolved)
    
    if type(exc.message) != requests.packages.urllib3.exceptions.MaxRetryError:
        raise
    reason = exc.message.reason    

    if type(reason) != requests.packages.urllib3.exceptions.NewConnectionError:
        raise
    
    if type(reason.message) != str:
        raise
    
    if ("[Errno 11001] getaddrinfo failed" in reason.message or # Windows
        "[Errno -2] Name or service not known" in reason.message or # Linux
        "[Errno 8] nodename nor servname " in reason.message):      # OS X
        message = 'DNSLookupError'
    else:
        raise

    print(message)