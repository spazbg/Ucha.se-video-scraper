# UCHA.SE VIDEO DOWNLOADER
A small batch of spaghetti code with the help of which you can generate text input for FFmpeg and download a video course from https://ucha.se/. The service uses encrypted HLS also know as HTTP Live Streaming.

The compiled .exe expect as input your email and password from your account and the course URL to be scraped e.g. https://ucha.se/videos/angliyski-ezik/nivo-b2/ 

(This is the URL with all videos from a given course)

<img src="https://i.imgur.com/vxabyJi.png">

<img src="https://i.imgur.com/jcJQIqj.png">

When the script is ready with his magic, he generates a download.txt file at his folder. 

<img src="https://i.imgur.com/Sr1zpmw.png">

<img src="https://i.imgur.com/6Zv3o9f.png">

The download.txt file contains a one-line command for ffmpeg.exe, which will download all the videos from a given course with proper numbering and names. Just copy/paste all the text (without the "====" lien separator at the end of the file) to ffmpeg.exe and you are done. 

<img src="https://i.imgur.com/ib4S31b.png">

<img src="https://i.imgur.com/pzQd8Vh.png">

In the end, you will get all the files from the course. 
<img src="https://i.imgur.com/DyyxaJy.png">

PS. If you use the script to download more then one video course either delete the download.txt files when the first download process end or use the lastest one-liner in the download.txt file. Every one-liner is separate by the previous with a row of "===========". 
