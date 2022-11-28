import numpy as np
import cv2
import math
import socket
import time

UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

last = []

# Open Camera
try:
    default = 0 # Try Changing it to 1 if webcam not found
    capture = cv2.VideoCapture(default)
    # img = cv2.imread('input/DSC_3589.JPG')
    # img = cv2.imread('input/DSC_3590.JPG')
    #img = cv2.imread('input/fcomp-04-826412-g004.jpg')
    img = cv2.imread('input/IMG_0151.JPG')

    # Scaling for JPG only
    # NEF requires different scaling, as those images come out in a much smaller resolution
    img = cv2.resize(img, None, fx=0.1, fy=0.1, interpolation = cv2.INTER_AREA)
    # Take only a subsect of the image
    # if we have the rings of the petri dish, we won't fill in the contours correctly
    #img = img[100:300,200:400]
    img = img[70:330,:]
    
except:
    print("No Camera Source Found!")

blendKernel = np.array([[0, -1, 0],
                            [-1, 5, -1],
                            [0, -1, 0]])

blended = cv2.filter2D(src=img, ddepth=-1, kernel=blendKernel)

# black and white
grey = cv2.cvtColor(cv2.cvtColor(blended, cv2.COLOR_BGR2GRAY), cv2.COLOR_GRAY2BGR)

cv2.imshow("raw", img)

# contour analysis
mask = cv2.Canny(grey, 50, 150)

cv2.imshow("mask", mask)

# close the contours by dilating and eroding
kernal = cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(6,6))
closed = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernal)
cv2.imshow("dilated", closed)

# reverse the colors
inverse = closed.copy()

h,w = inverse.shape
for i in range(h):
    for j in range(w):
        inverse[i, j] = 255 - inverse[i, j]

# flood fill the background
h,w = closed.shape
_, filled, _, _ = cv2.floodFill(closed, None, (0,0), 255)
_, filled, _, _ = cv2.floodFill(filled, None, (0,h-1), 255)
_, filled, _, _ = cv2.floodFill(filled, None, (w-1,0), 255)
_, filled, _, _ = cv2.floodFill(filled, None, (w-1,h-1), 255)

cv2.imshow("inverse", inverse)
cv2.imshow("filled", filled)

# combine the flooded image and the inverse image
# result = cv2.bitwise_and(filled, inverse)

#cv2.imshow("result", result)

cv2.imshow("masked result", cv2.bitwise_and(img, img, mask=filled))

colored = cv2.bitwise_and(blended, blended, mask=filled)

cv2.imshow("colored", colored)

# blur the edges
# this will create a smoother fall-off for plant generation
# like the outskirts of a forest
blurred = cv2.blur(filled, (10,10));
cblurred = cv2.blur(colored, (10,10));

# resize the image to be 100x100
resized = cv2.resize(blurred, (100, 100), interpolation = cv2.INTER_AREA)
cresized = cv2.resize(cblurred, (100, 100), interpolation = cv2.INTER_AREA)

cv2.imshow("blended", blended)

cv2.imshow("blurred", blurred)
cv2.imshow("cblurred", cblurred)


cv2.imwrite("../Assets/Resources/plant_output.png", resized)
cv2.imwrite("../Assets/Resources//creature_output.png", cresized)

while capture.isOpened():
    
    # Capture frames from the camera
    #ret, frame = capture.read()
    
    #mask = cv2.Canny(frame, 150, 200)

    #cv2.imshow("Image", mask)
    #cv2.imwrite("G:\\Users\\cirea\\Documents\\Blackboard Assignment\\GameAI\\Assets\\Resources\\output.png", mask)

    # Close the camera if 'q' is pressed
    if cv2.waitKey(1) == ord('q'):
        break

capture.release()
cv2.destroyAllWindows()