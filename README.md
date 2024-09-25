# Networking
My CMP303 Third Year Networking project. Simple demonstration project of two clients capable of moving in sync over a .NET server, using Interpolation and Prediction techniques, and both transport layer protocols TCP and UDP. 

*Image of two clients connected to same server*
![image](https://github.com/user-attachments/assets/42080ce8-8344-47f4-bb1c-c8d2c712da77)

## Instructions For Use
First, install both the Client and the Server from the download link.
Then, ensure that the Server runs first, before attempting to connect the Client.

## Technical Explanation
This project involves both a server and a client, making use of both TCP and UDP. TCP is used for highly important information that cannot be lost, for example the client's username. UDP is used to transmit the client's next movement package, which can afford to be lost as one is sent every 100 milliseconds. 

Prediction and Linear Interpolation are used on the client's movement data, to ensure a smoother experience for other clients. Without these techniques, any other clients on the server would appear to teleport every 100 milliseconds rather than move smoothly, and would always end up being one packet behind. This would lead to the local client always recieving old movement data, rather than current movement data. Prediction helps remedy this.

Prediction works by taking an educated guess based on the last two movement packages, where the next position of the other clients will be, and will pre-emptively move the client to that location. If the prediction is correct (based on when the next movement data is recieved) then that client will continue moving as usual. If the prediction is wrong however, the local client will create a new prediction for that client, and lerp to that new predicted position, from the current on-screen position of the connected client. This results in smooth movement that, even when wrong, can correct itself.

*Diagram of Prediction*

![Prediction_Diagram](https://github.com/user-attachments/assets/7f64365d-efcd-419a-85bd-59297aa64ed4)

## Links
Link to download: https://drive.google.com/drive/folders/1swUEBBun0nNLvd-28Qj33ep4k7SLlCN_?usp=share_link
