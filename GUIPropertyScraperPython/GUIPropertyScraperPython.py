import wpf
import requests

from System.Windows import Application, Window
from bs4 import BeautifulSoup as bs


baseUrl = "https://www.rightmove.co.uk"
headers = {'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36'}
htmlParser = "html.parser"

class PropertyDataModel(object):
    def __init__(self, propertyAddress, propertyType, propertyPrice, propertyMainPicture):
        self.PropertyAddress = propertyAddress
        self.PropertyType = propertyType
        self.PropertyPrice = propertyPrice
        self.PropertyMainPicture = propertyMainPicture

def GetIndividualPropertyData():
    try:
        for item in Web_Section_Of_Interest:
            print("Fatching data from: " +str(baseUrl + item.get('href')))
            rawPropertyData = requests.get(baseUrl + item.get('href'),headers=headers).text
            htmPropertyData = bs(rawPropertyData,htmlParser)

            print(htmPropertyData.find_all('h1',class_="fs-22"))

            propertyType = htmPropertyData.find_all('h1',{'class':'fs-22', 'itemprop': 'name'})[0].text
            propertyAddress = htmPropertyData.find('meta',{'itemprop': 'streetAddress'})['content']

            aa= htmPropertyData.find_akk('small',{'class':'property-header-qualifier'})

            print(aa)

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

class MyWindow(Window):
    def __init__(self):
        wpf.LoadComponent(self, 'GUIPropertyScraperPython.xaml')
        
    def Button_Click(self, sender, e):
        if url != "": 
            print("Fatching data form " + url)

            Web_Page = requests.get(url)
            Soup = bs(Web_Page.text,htmlParser)
            Web_Section_Of_Interest = Soup.find_all('a',class_="propertyCard-img-link")

            print('Fatching from: '+url+'completed successfully')
            
            GetIndividualPropertyData()
        else:
            print("Url is not valid")
        pass


if __name__ == '__main__':
    Application().Run(MyWindow())
