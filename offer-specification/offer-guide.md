---
visibility: internal
---

Offer Management API v2
=========================
Offer Management API v2 lets you initiate, track and execute offers for loans, deposits and accounts. It provides simulation of conditions for potential products and services.

Main Diferences between Offer API v1 and Offer API v2
-------------
Offer Management has tree top level collection resources: applications, calculation and classification.  

Key Resources
-------------
Offer Management has tree top level collection resources: applications, calculation and classification.  

Resource            | Description
--------------------|-----------
*applications*      | Workitem used to capture details of client request for a product or service. Serves as a ticket for tracking progression of the workflow through phases and states, from draft to closed.
*calculations*      | Represents the result of calculating; determining terms, fees and taxes by mathematical or logical methods, like calculate service fee.
*classifications*   | Represents classification schemes and values. Systems of organizing data or information, usually involving categories of items with similar characteristics.   


Getting started tutorial
---------------
To get started follow these steps:
###1. Authenticate your app
Offer API uses OAuth 2.0 for authentication. You get an access token that authenticates your app with a particular set of permissions for a user. You provide an access token through an HTTP header:
```
Authorization: bearer {token}
```
To obtain an access token and sign the user in, see [authentication]() section.

###2. URL Root
Now that you've authenticated your app, you can call the Offer API with your access token against the URL root below, combined with one of the root resources.  Offer API URLs are relative to the following root unless otherwise noted.

API | URL Root
--------|---------
Offer      | `https://bankapi.net/offer`

> **Note**: Throughout this documentation, only partial syntax such as:
`GET /applications/{application-number}` is used for the sake of brevity.
Prefix the path with the correct root URL in order to obtain the full resource path or URL.


###3. Create current account opening
Let's assume that you want to create current account opening for fully registered customer.
To create current account request you need to post `current account opening` metadata representation as json to following endpoint:

```
POST /applications/
```

```json
{
  "currency": "RSD",
  "customer-number": "0000078",
  "channel": "branch",
  "product-code": "0009",
  "product-name": "Current account"
}
```

You will get back `200` status code and workitem number of created current account opening.

```json
{
    "application-number": "0000000057"
}
```


###4. Retreive created request details

You can get created current account opening details at following endpoint:

```
GET /applications/0000000057
```

You will get back `200 OK` status code and json representation of created current account opening.

```json
{
    "application-number": "0000000057",
    "status": "draft",
    "product-code": "0009",
    "product-name": "Current account",
    "customer-number": "0000078",
    "customer-name": "Jovana Nikolić",
    "channel-code": "branch",
    "initiator": "test",
    "preffered-culture": "sr-Latn-RS",
    "status-information": {
        "description": "We are finalizing your application. You will be notified about next steps.",
        "title": "Application Finalization",
        "html": ""
    },
    "request-date": "2018-10-19T07:24:04.407385",
    "term-limit-breached": false,
    "amount-limit-breached": false,
    "preferencial-price": false
}
```
###5. List customer requests
Now lets list the requests for customer and verify that it contains created current account opening.

```
GET /applications?customer-data=0000078
```

You will get back  `200 OK` status code and json representation with a list of product account openings.

```json
{
    "applications": [
        {
            "application-number": "0000000057",
            "status": "draft",
            "product-code": "0009",
            "product-name": "Current account",
            "customer-number": "0000078",
            "customer-name": "Jovana Nikolić",
            "channel-code": "branch",
            "initiator": "test",
            "preffered-culture": "sr-Latn-RS",
            "status-information": {
                "description": "We are finalizing your application. You will be notified about next steps.",
                "title": "Application Finalization",
                "html": ""
            },
            "request-date": "2018-10-19T07:24:04.407385",
            "term-limit-breached": false,
            "amount-limit-breached": false,
            "preferencial-price": false
        }
    ],
    "total-count": 1,
    "page-size": 10,
    "page": 1,
    "total-pages": 1
}
```



**Congratulations!** You have completed getting started tutorial on most common steps when working with Offer Management API. To learn more look at the reference documentation for [available operations](swagger-ui).