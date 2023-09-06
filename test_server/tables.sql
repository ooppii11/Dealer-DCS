CREATE TABLE IF NOT EXISTS USERS(
    username TEXT UNIQUE,
    password TEXT,
    first_location TEXT,
    second_location TEXT,
    third_location TEXT
);

