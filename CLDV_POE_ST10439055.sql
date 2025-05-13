USE master 
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'EventEaseDB')
DROP DATABASE EventEaseDB
CREATE DATABASE EventEaseDB

USE EventEaseDB

CREATE TABLE Venues (
    VenueId INT PRIMARY KEY IDENTITY(1,1),
    VenueName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Capacity INT NOT NULL,
    ImageUrl NVARCHAR(500)
);

CREATE TABLE Events (
    EventId INT PRIMARY KEY IDENTITY(1,1),
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATETIME NOT NULL,
    Description NVARCHAR(MAX),
    VenueId INT NULL,
    CONSTRAINT FK_Event_Venue FOREIGN KEY (VenueId) REFERENCES Venues(VenueId)
);

CREATE TABLE Bookings (
    BookingId INT PRIMARY KEY IDENTITY(1,1),
    EventId INT NOT NULL,
    VenueId INT NOT NULL,
    BookingDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Booking_Event FOREIGN KEY (EventId) REFERENCES Events(EventId),
    CONSTRAINT FK_Booking_Venue FOREIGN KEY (VenueId) REFERENCES Venues(VenueId),
    CONSTRAINT UQ_Event_Venue UNIQUE (EventId, VenueId)
);


INSERT INTO Venues (VenueName, Location, Capacity, ImageUrl)
VALUES 
('Grand Ballroom', '123 Main Street, New York', 500, 'https://example.com/ballroom.jpg'),
('City Convention Center', '456 Oak Avenue, Chicago', 1200, 'https://example.com/convention.jpg'),
('Riverside Theater', '789 Broadway, Los Angeles', 350, 'https://example.com/theater.jpg'),
('Garden Pavilion', '101 Park Lane, Miami', 250, 'https://example.com/pavilion.jpg'),
('Skyline Arena', '202 High Street, Seattle', 8000, 'https://example.com/arena.jpg');


INSERT INTO Events (EventName, EventDate, Description, VenueId)
VALUES
('Tech Conference 2023', '2023-11-15 09:00:00', 'Annual technology conference featuring top speakers', 2),
('Summer Music Festival', '2023-07-22 18:00:00', 'Outdoor music festival with multiple stages', 5),
('Art Exhibition Opening', '2023-09-05 17:30:00', 'Contemporary art exhibition opening night', 3),
('Business Leadership Summit', '2023-10-10 08:30:00', 'Two-day summit for business executives', 1),
('Charity Gala Dinner', '2023-12-12 19:00:00', 'Annual fundraiser for local children''s hospital', 4);


INSERT INTO Bookings (EventId, VenueId, BookingDate, CreatedDate)
VALUES
(1, 2, '2023-01-15 10:00:00', '2023-01-15 10:00:00'),
(2, 5, '2023-02-20 11:30:00', '2023-02-20 11:30:00'),
(3, 3, '2023-03-10 09:15:00', '2023-03-10 09:15:00'),
(4, 1, '2023-04-05 14:00:00', '2023-04-05 14:00:00'),
(5, 4, '2023-05-18 16:45:00', '2023-05-18 16:45:00');