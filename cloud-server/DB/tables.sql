CREATE TABLE IF NOT EXISTS users(
    id SERIAL PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    phone_number TEXT
);

CREATE TABLE IF NOT EXISTS file_metadata (
    id SERIAL PRIMARY KEY,
    creator_id int NOT NULL,
    name TEXT NOT NULL,
    size INT NOT NULL,
	type TEXT NOT NULL,
    creation_time TIMESTAMP WITH TIME ZONE DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
	last_modify TIMESTAMP WITH TIME ZONE DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),

    FOREIGN KEY (creator_id) REFERENCES users(id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS file_location (
    file_id int NOT NULL,
    primary_server_ip TEXT NOT NULL,
    backup_server_ip_1 TEXT NOT NULL,
    backup_server_ip_2 TEXT NOT NULL,

    FOREIGN KEY (file_id) REFERENCES file_metadata(id) ON DELETE CASCADE
);

CREATE OR REPLACE FUNCTION insertFileMetadata(
    creator_id_param INTEGER,
    name_param TEXT,
    file_type_param TEXT,
    size_param INTEGER,
    OUT result INTEGER
)
AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM file_metadata
        WHERE creator_id = creator_id_param AND name = name_param
    )
    THEN
        INSERT INTO file_metadata (creator_id, name, type, size)
        VALUES (creator_id_param, name_param, file_type_param, size_param)
        RETURNING id INTO result;
    ELSE
        RAISE EXCEPTION 'File with creator_id = % and name = % already exists', creator_id_param, name_param;
    END IF;
END;
$$ LANGUAGE PLPGSQL;