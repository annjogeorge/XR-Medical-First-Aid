import cv2
import mediapipe as mp
import time
import math
import socket  # Required for UDP communication

# --- UDP SETUP ---
UDP_IP = "127.0.0.1"  # Localhost (Same laptop)
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Initialize MediaPipe Pose
mp_pose = mp.solutions.pose
pose = mp_pose.Pose(min_detection_confidence=0.7, min_tracking_confidence=0.7)
mp_draw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0) # Change to 1 if using external webcam

count = 0
direction = 0 
prev_y = 0
hand_distance_threshold = 0.1 
depth_threshold = 0.05 

print("Starting CPR Analysis... Sending data to Unity on Port 5005")

while cap.isOpened():
    success, img = cap.read()
    if not success: break
    
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = pose.process(img_rgb)
    
    if results.pose_landmarks:
        lm = results.pose_landmarks.landmark
        l_wrist = lm[mp_pose.PoseLandmark.LEFT_WRIST]
        r_wrist = lm[mp_pose.PoseLandmark.RIGHT_WRIST]
        
        dist = math.sqrt((l_wrist.x - r_wrist.x)**2 + (l_wrist.y - r_wrist.y)**2)
        
        if dist < hand_distance_threshold:
            avg_y = (l_wrist.y + r_wrist.y) / 2
            
            if prev_y != 0:
                if avg_y > prev_y + depth_threshold and direction == 0:
                    direction = 1
                
                if avg_y < prev_y - depth_threshold and direction == 1:
                    count += 1
                    direction = 0
                    # --- SEND SIGNAL TO UNITY ---
                    sock.sendto("1".encode(), (UDP_IP, UDP_PORT))
                    print(f"Valid Compression! Sent to Unity. Total: {count}")
            
            prev_y = avg_y
            status_text = "Hands Locked"
            color = (0, 255, 0)
        else:
            status_text = "Position Error"
            color = (0, 0, 255)

        cv2.putText(img, status_text, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, color, 2)
        cv2.putText(img, f"Count: {count}", (50, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2)

    cv2.imshow("CPR Performance Evaluator", img)
    if cv2.waitKey(1) & 0xFF == ord('q'): break

cap.release()
cv2.destroyAllWindows()