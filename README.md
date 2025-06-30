Serverless & Cloud-native Approach (AWS)

Components:
Data Ingestion:
AWS Lambda (scheduled hourly via CloudWatch Events)
Fetches data from Polygon.io or other news providers.
Handles API key management via AWS Secrets Manager.

Data Enrichment:
AWS Lambda function:
Fetches ticker details, charts, company info via additional API calls (Polygon.io, Yahoo Finance API, etc.).

Data Storage:
AWS DynamoDB (for metadata/news items, quick access & querying)
AWS S3 (storage for enriched items, images, charts, historical data, etc.)

API Layer:
AWS API Gateway + AWS Lambda
Implemented as RESTful API endpoints.
Authorization via Amazon Cognito for protected endpoints.

API endpoints:
[Authorize] GET /news: Return all news items.
[Authorize] GET /news?days={n}: Return news from today minus n days.
[Authorize] GET /news/{instrument}?limit={limit}: Return news per instrument.
[Authorize] GET /news/search?text={text}: Search news containing specific text.
[Authorize] POST /subscribe: Allow customers to subscribe to updates (stored in DynamoDB).
[Public] GET /latest-news: Provide latest 5 distinct instruments for public conversion tools.

Advantages:
Scalable & cost-effective.
Minimal infrastructure maintenance.
Highly reliable and managed security via AWS IAM & Cognito.

Disadvantages:
Vendor lock-in.
Cold-start latency for Lambdas (minimal).
