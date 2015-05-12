## EyePower
Analyze Any Image With Single Click.
Built on Top of [Microsoft Vision API](https://www.projectoxford.ai/vision) And [Face++ Detection API](http://www.faceplusplus.com/), [Recognize Image Tagging API](https://rekognition.com/developer/scene) and [Alchemy Image Tagging API](http://www.alchemyapi.com/api/image-tagging/image.html)

### > [Download Installer](https://goo.gl/3hpClP)
### > [Check The App On SoftPedia](http://www.softpedia.com/get/Multimedia/Graphic/Graphic-Others/Eye-Power.shtml)

## How To Use:
1-After Downloading the installer the app needs to run for the first time as Administrator to create some keys in Windows Registry.

2-There are two possible ways to capture the image content:

 *Right Click on any image and click "Read Image Content"
 
 *Open The App and choose the URL or the Location of the Image.
 
3-After analyzing the image you'll see the result on the Log TextBox.


# Note:
To rebuild this project again you need to have subscription key for the Vision API and Get The API Key and API Secret for Face++ API, Rekognize API, Alchemy API and Cloudinary API. You can get the Vision Subscription Key from [Here](https://www.projectoxford.ai/vision), Face++ API Key And API Secret from [Here](http://www.faceplusplus.com/), Rekognize API Key and API Secret From [Here](https://rekognition.com/developer/scene) ,Alchemy API Key from [Here](http://www.alchemyapi.com/api/image-tagging/image.html) and Cloudinary API Key and API Secret From [Here](http://cloudinary.com/). When you get the keys replace the <Subscription_Key> string in the Detect/{API-Name} with your new subscription key and <API_KEY> and <API_SECRET> with your new API Keys and for Cloudinary you'll find the <API_KEY> and <API_SECRET> in Form1.CS Page.

# Why do I Use 3 Services For Image Tagging ?
Everyone of them has a limit for free access.
Alchemy can give you 1000 transactions/day. Rekognize gives you 660 transactitions/day and vision gives you 150 transactions/day.
All of their results are pretty close.
So I use Alchemy 10 times then Rekognize 5 times then Vision 1 time only and then get back to Alchemy.
using this method I can do Image tagging 1810 Image tagging requests per day for free.
You'll find the algorithm for this technique in /Detect/Decider.cs File.


> Works On Windows Vista SP2, 7 , 8 Since The .NET 4.5 is Supported On It.



### Application Main Window : 
![Eye Power Main Window](https://github.com/ShawkyZ/EyePower/blob/master/Screenshot1.png)
