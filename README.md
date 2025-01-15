# CoreConnect

Welcome to **CoreConnect**! This is a fully functional social media platform developed as a project for our *Development of Web Applications* class during the second year of college. CoreConnect is designed to provide a streamlined, engaging social experience with a variety of features that enhance connectivity and interaction among users.

---

## Features

### **User Authentication**
- Secure sign-up and login system.
- Password hashing for user safety.
- Persistent session management.

### **Explore Page**
- Discover posts from public profiles and users you follow.
- Dynamic feed populated with engaging content.
![image](https://github.com/user-attachments/assets/aec7548c-4f9f-4867-b8d5-2039d20acdb4)


### **Home Page**
- View posts exclusively from users you follow.
- Personalized content for a tailored experience.

### **Posts**
- Create posts with text and media attachments.
- Like and comment on posts to engage with the community.

### **Profiles**
- Customize your profile with the following options:
  - First Name and Last Name.
  - Bio and Profile Picture.
  - Privacy Settings: Public or Private profiles.
- Send, receive, accept, or reject follow requests for private profiles.
![image](https://github.com/user-attachments/assets/b0d0ec53-919e-41a7-bea6-e1ce58660bad)


### **Groups**
- Create and join groups to connect with like-minded individuals.
- Send, edit, or delete messages in real-time within groups.
- Powered by **SignalR** for real-time communication.
![image](https://github.com/user-attachments/assets/f0cc3b9a-4c95-4d17-a1dc-2799d6c32f4e)


### **Other Features**
- Responsive and visually appealing UI with **Bootstrap**.
- Deployed on **Azure** for high availability and scalability.

---

## Tech Stack

CoreConnect is built using the following technologies:

- **C#** and **ASP.Net Core**: For backend logic and API development.
- **HTML**, **CSS**, **JavaScript**, and **Bootstrap**: For front-end design and interactivity.
- **SignalR**: To enable real-time messaging in groups.
- **Azure**: Cloud hosting and deployment for the application.

---

## Installation

To set up CoreConnect locally, follow these steps:

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Androidus2/CoreConnect.git
   ```

2. **Install Dependencies**:
   Install Visual Studio Community with the "ASP.NET and web development" and "Data storage and processing" modules installed. Alternatively, you can install these manually, but configuring them won't be the same.

3. **Configure the Database**:
   - Update the connection string in `appsettings.json` to point to your local database.
   - Run Entity Framework migrations.

4. **Run the Application**:
   - Press run in visual studio to test the application.

---

## Deployment

CoreConnect is deployed on **Azure** for live access. You can visit the application at:

[CoreConnect on Azure](https://aspmicrosocialplatform20241215125251.azurewebsites.net/)

---

## Contributors

- **[Turculeț Alexandru-Ioan](https://github.com/Androidus2/)**
- **[Borozan George](https://github.com/Geutzzu)**

