CREATE TABLE sales (
    id SERIAL PRIMARY KEY,
    company_id INT,
    description VARCHAR(255),
    gross_amount NUMERIC(10, 2),
    tax_amount NUMERIC(10, 2),
    sales_date DATE
);


INSERT INTO sales (company_id, description, gross_amount, tax_amount, sales_date)
SELECT
    1,
    'Lorem ipsum dolor siamet',
    (RANDOM() * 1000)::NUMERIC(10, 2),
    (RANDOM() * 100)::NUMERIC(10, 2),
    CURRENT_DATE - (RANDOM() * 365)::INT
FROM
    generate_series(1, 10000000);

CREATE INDEX idx_company_sales_date ON sales (company_id, sales_date);