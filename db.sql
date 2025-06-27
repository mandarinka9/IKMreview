-- Таблица genres
drop table book_authors;
drop table authors;

drop table books;
drop table genres;

CREATE TABLE genres (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE CHECK (char_length(name) > 0)
);

-- Таблица books
CREATE TABLE books (
    id UUID PRIMARY KEY,
    title VARCHAR(255) NOT NULL CHECK (char_length(title) > 0),
    description TEXT,
    genre_id INT REFERENCES genres(id) ON DELETE CASCADE,
    is_available BOOLEAN DEFAULT TRUE,
    publication_date DATE CHECK (publication_date <= CURRENT_DATE),
    popularity_score REAL CHECK (popularity_score BETWEEN 0 AND 10),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);


-- Таблица authors
CREATE TABLE authors (
    id UUID PRIMARY KEY,
    surname VARCHAR(50) NOT NULL CHECK (char_length(surname) > 0),
    name VARCHAR(255) NOT NULL CHECK (char_length(name) > 0),
    patronymic VARCHAR(255),
    birth_date DATE CHECK (birth_date <= CURRENT_DATE),
    biography TEXT
);


-- Таблица book_authors
CREATE TABLE book_authors (
    book_id UUID NOT NULL REFERENCES books(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES authors(id) ON DELETE CASCADE,
    PRIMARY KEY (book_id, author_id)
);


INSERT INTO genres (name) VALUES
    ('Fiction'),
    ('Science Fiction'),
    ('Fantasy'),
    ('Mystery'),
    ('Romance'),
    ('Thriller'),
    ('Horror'),
    ('Historical'),
    ('Biography'),
    ('Self-Help');

INSERT INTO books (id, title, description, genre_id, is_available, publication_date, popularity_score, created_at)
VALUES
    (gen_random_uuid(), 'Book 1', 'Description for Book 1', 1, true, '2023-01-01', 8.5, now()),
    (gen_random_uuid(), 'Book 2', 'Description for Book 2', 2, true, '2023-02-01', 7.2, now()),
    (gen_random_uuid(), 'Book 3', 'Description for Book 3', 3, false, '2023-03-01', 6.8, now()),
    (gen_random_uuid(), 'Book 4', 'Description for Book 4', 4, true, '2023-04-01', 9.1, now()),
    (gen_random_uuid(), 'Book 5', 'Description for Book 5', 5, true, '2023-05-01', 7.7, now()),
    (gen_random_uuid(), 'Book 6', 'Description for Book 6', 6, false, '2023-06-01', 8.3, now()),
    (gen_random_uuid(), 'Book 7', 'Description for Book 7', 7, true, '2023-07-01', 6.5, now()),
    (gen_random_uuid(), 'Book 8', 'Description for Book 8', 8, true, '2023-08-01', 7.9, now()),
    (gen_random_uuid(), 'Book 9', 'Description for Book 9', 9, false, '2023-09-01', 9.0, now()),
    (gen_random_uuid(), 'Book 10', 'Description for Book 10', 10, true, '2023-10-01', 8.0, now());



INSERT INTO authors (id, surname, name, patronymic, birth_date, biography)
VALUES
    (gen_random_uuid(), 'Smith', 'John', 'Edward', '1980-01-15', 'Biography of John Smith'),
    (gen_random_uuid(), 'Doe', 'Jane', 'Marie', '1990-02-10', 'Biography of Jane Doe'),
    (gen_random_uuid(), 'Brown', 'James', 'Patrick', '1985-03-20', 'Biography of James Brown'),
    (gen_random_uuid(), 'Johnson', 'Emily', 'Sophia', '1975-04-25', 'Biography of Emily Johnson'),
    (gen_random_uuid(), 'Williams', 'Michael', 'David', '1988-05-30', 'Biography of Michael Williams'),
    (gen_random_uuid(), 'Taylor', 'Sarah', 'Ann', '1992-06-05', 'Biography of Sarah Taylor'),
    (gen_random_uuid(), 'Anderson', 'Robert', 'John', '1970-07-12', 'Biography of Robert Anderson'),
    (gen_random_uuid(), 'Thomas', 'Laura', 'Elizabeth', '1983-08-20', 'Biography of Laura Thomas'),
    (gen_random_uuid(), 'Moore', 'Daniel', 'James', '1995-09-25', 'Biography of Daniel Moore'),
    (gen_random_uuid(), 'Jackson', 'Sophia', 'Grace', '2000-10-10', 'Biography of Sophia Jackson');

SELECT * FROM books;
SELECT * FROM authors;

INSERT INTO book_authors (book_id, author_id)
VALUES
    ('fe4ba608-ed2b-414b-aa96-c44f9a871aaa', 'f4568a07-effd-43eb-9bde-48128bae9dc3'),
    ('d88e3c3c-6cab-497a-b283-719eac08bc61', 'd1d41a22-f0ba-4969-88a7-930f9cf4fbd1'),
    ('d88e3c3c-6cab-497a-b283-719eac08bc61', '94602ac0-8f03-4305-b307-f918e2e0352b'),
    ('14f38801-ef93-4243-9181-ee23374fd934', 'ed7b36d6-f019-4c10-b687-33fe8320f6d7'),
    ('7c2293ff-0b94-42f3-89d6-5f969c55da8c', 'ed7b36d6-f019-4c10-b687-33fe8320f6d7'),
    ('3a421000-a995-4ba5-8574-fe8f33878f77', '801675ab-4b83-40b2-a87e-0919f4dcf0a9'),
    ('807a88e1-e9ed-43e6-89b8-282775e5f2ed', '013c658e-5284-4322-ac3e-d3543af3ece6'),
    ('43774111-ef34-4cec-94d5-62126fd7dd2b', 'c16805f1-756a-4b88-94d9-ba0af3a7ea2e'),
    ('ef3aaec3-f89d-4dd5-92a7-dee000436cef', '3b33cd9f-00d2-4cf7-81d7-2cd2dcf22e33'),
    ('1340d504-ce91-4484-8a57-037febead9ec', 'e5c7ca93-e413-4db6-b09b-cb872d99e3b1');








SET lc_monetary TO 'en_US.UTF-8';