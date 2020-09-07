# Offer

This repository contains Offer Service (Offer Api) prototype implemented according to architectural requirements (onion architecture).
Offer Service DAL is implemented using Repository pattern and Entity Framework. 

# Architecture

This service is developed as an implementation example of CQRS and DDD recommended architecture for microservices.

## Project structure

**Offer**

- API implementation of Offer

**Offer.Infrastructure**

- Database configuration, infrastructure configuration setup

**Offer.Domain**

- Domain models

# Features

## Price Calculation

Price calculationis used to resolve DMN rules and create product defined price variations. Variable prices are interest rates and fees' percentage.

### Pricing rule currency

Every price variation (fees and interest rates) has property ```pricing-rule-currency``` which represents currency of ```amountInRuleCurrency``` property in DMN tables. This is used for standardizing DMN amount values regardless of selected currency on arrangement request.

Example:
In DMN table field ```amount``` is available, and arrangement request has currency as *EUR* available in DMN table as ```currency``` field.
Bank wants to define price variation on regular interest of **-0.2%** if loan is greater than **1.000.000.00 RSD**.
We have ```amount``` in **EUR**, but condition in **RSD**. Here ```amountInRuleCurrency``` steps in and we can use it in rules instead of ```amount```.

### DMN rules

Every DMN table must have output fields named ```variationGroup```, ```variationDescription``` and ```percentage``` in order price calculation to work.
Input fields available for use in rules are as follows:

- channel
- customerSegment
- currency
- amount
- term
- collateralModel
- riskScore
- productBundling
- amountInRuleCurrency

These DMN tables are stored in ```product/price-variations``` configuration parent key.

Default price variation parameters are stored on configuration as key ```price-calculation/defaults```.
Example of default parameters:

```json
{
    "collateral-model": "co-debtor",
    "product-bundling": "0001",
    "risk-score": 1,
    "customer-segment": "professional"
}
```

# API 

This API is implemented according to Offer V2 specification. Currently this specification is not finished yet.

## Future work

Some parts of this API will be in the seperate NuGet package for global usage in other microservices. 

# Docker 

Inside this repository there is [DOCKER.md](DOCKER.md) specifiying all environment variables for Offer Container. In this file you may also find samples for running Offer as Container.

# Development

To develop on Offer API you need to run MSSQLServer on localhost with next command on Docker:

```docker run --name mssql -eACCEPT_EULA=Y -eSA_PASSWORD=Sifra123# -eMSSQL_PID=Express -p1433:1433 -d microsoft/mssql-server-linux:2017-latest```

and also you need to run digitalo-broker:

```docker run --name broker -eENABLE_JMX=true -eJMX_PORT=1199 -eJMX_RMI_PORT=1198 -eARTEMIS_USERNAME=admin -eARTEMIS_PASSWORD=admin -eTOPICS=offer,bpm,product,party,notifications,errors -p61613:61613 -p5672:5672 -p61616:61616 -p8161:8161 -p61623:61623 -d registry.asseco.rs/asseco/digitalo-broker``` 

After running these two containers you can start the Offer API and start development.

# Api call examples

## /calculation

Calculate one of Annuity, Term or Amount based on other 2 elements, nominal interest rate and fees. Method returns Arrangement Request with Installement plan.

Example of Annuity calculation payload:

```json
{
  "currency": "EUR",
  "amount": 35000,
  "term": 240,
  "interest-rate": 4.66,
  "fees": [
    {
      "kind": "origination-fee",
      "frequency": "event-triggered",
      "service-code": "",
      "service-description": "",
      "percentage": 1.29,
      "fixed-amount": 7500.0,
      "fixed-amount-currency": "RSD",
      "variations-definition": "product/price-variations/origination-fee-variations.dmn",
      "lower-limit-amount": 1500.0,
      "lower-limit-currency": "RSD",
      "upper-limit-amount": 100000.0,
      "upper-limit-currency": "RSD",
      "percentage-lower-limit": 0.01,
      "percentage-upper-limit": 5.0,
      "title": "Origination fee",
      "effective-date": "2012-01-01T00:00:00",
      "currencies": [
        "RSD",
        "EUR"
      ],
      "pricing-rule-currency": null
    }
  ],
  "interest-rates": [
    {
      "kind": "regular-interest",
      "calendar-basis": "act-act-isda",
      "is-compound": true,
      "rate": {
        "base-rate-id": "EURIBOR-3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 6.5
      },
      "variations-definition": "product/price-variations/interest-rate-variations-car-loan.dmn",
      "lower-limit": {
        "base-rate-id": "EURIBOR-3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 5.5
      },
      "lower-limit-variations-definition": "product/price-variations/interest-rate-lower-limit-variations.dmn",
      "upper-limit": {
        "base-rate-id": "EURIBOR3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 12
      },
      "upper-limit-variations-definition": null,
      "calculated-rate": 6.5,
      "title": "Regular interest",
      "effective-date": "2012-01-01T00:00:00",
      "currencies": [
        "EUR"
      ],
      "pricing-rule-currency": null
    }
  ]
}
```

Example of Term calculation payload:

```json
{
  "currency": "EUR",
  "amount": 35000,
  "annuity": 200,
  "interest-rate": 4.66,
  "channel": "web",
  "customer-segment": "student",
  "collateral-model": "co-debtor",
  "fees": [
    {
      "kind": "origination-fee",
      "frequency": "event-triggered",
      "service-code": "",
      "service-description": "",
      "percentage": 1.29,
      "fixed-amount": 7500.0,
      "fixed-amount-currency": "RSD",
      "variations-definition": null,
      "lower-limit-amount": 1500.0,
      "lower-limit-currency": "RSD",
      "upper-limit-amount": 100000.0,
      "upper-limit-currency": "RSD",
      "percentage-lower-limit": 0.01,
      "percentage-upper-limit": 5.0,
      "title": "Origination fee",
      "effective-date": "2012-01-01T00:00:00",
      "currencies": [
        "RSD",
        "EUR"
      ],
      "pricing-rule-currency": null
    }
  ],
  "interest-rates": [
    {
      "kind": "regular-interest",
      "calendar-basis": "act-act-isda",
      "is-compound": true,
      "rate": {
        "base-rate-id": "EURIBOR-3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 6.5
      },
      "variations-definition": "product/price-variations/interest-rate-variations-car-loan.dmn",
      "lower-limit": {
        "base-rate-id": "EURIBOR-3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 5.5
      },
      "lower-limit-variations-definition": "product/price-variations/interest-rate-lower-limit-variations.dmn",
      "upper-limit": {
        "base-rate-id": "EURIBOR3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 12
      },
      "upper-limit-variations-definition": null,
      "calculated-rate": 6.5,
      "title": "Regular interest",
      "effective-date": "2012-01-01T00:00:00",
      "currencies": [
        "EUR"
      ],
      "pricing-rule-currency": null
    }
  ]
}
```

Example of Ammount calculation payload:

```json
{
  "currency": "EUR",
  "term": 250,
  "annuity": 200,
  "interest-rate": 4.66,
  "fees": [
    {
      "kind": "origination-fee",
      "frequency": "event-triggered",
      "service-code": "",
      "service-description": "",
      "percentage": 1.29,
      "fixed-amount": 7500.0,
      "fixed-amount-currency": "RSD",
      "variations-definition": "product/price-variations/origination-fee-variations.dmn",
      "lower-limit-amount": 1500.0,
      "lower-limit-currency": "RSD",
      "upper-limit-amount": 100000.0,
      "upper-limit-currency": "RSD",
      "percentage-lower-limit": 0.01,
      "percentage-upper-limit": 5.0,
      "title": "Origination fee",
      "effective-date": "2012-01-01T00:00:00",
      "currencies": [
        "RSD",
        "EUR"
      ],
      "pricing-rule-currency": "RSD"
    }
  ],
  "interest-rates": [
    {
      "kind": "regular-interest",
      "calendar-basis": "act-act-isda",
      "is-compound": true,
      "rate": {
        "base-rate-id": "EURIBOR-3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 6.5
      },
      "variations-definition": "product/price-variations/interest-rate-variations-car-loan.dmn",
      "lower-limit": {
        "base-rate-id": "EURIBOR-3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 5.5
      },
      "lower-limit-variations-definition": "product/price-variations/interest-rate-lower-limit-variations.dmn",
      "upper-limit": {
        "base-rate-id": "EURIBOR3M",
        "base-rate-value": -0.29,
        "spread-rate-value": 12
      },
      "upper-limit-variations-definition": null,
      "calculated-rate": 6.5,
      "title": "Regular interest",
      "effective-date": "2012-01-01T00:00:00",
      "currencies": [
        "EUR"
      ],
      "pricing-rule-currency": null
    }
  ]
}
```

## /applications/apply-online
Method initiate new applicaton. Returns application number.

Example of payload:

```json
{
  "mobile-phone": "+381 633361588",
  "email-address": "petar.petrovic@mail.com",
  "product-code": "0005",
  "product-name": "Mortgage loan",
  "given-name": "Petar",
  "surname": "Petrović",
  "parent-name": "Janko",
  "identification-number-kind": "personal-id-number",
  "identification-number": "2403983715068",
  "currency": "EUR",
  "amount": "60000",
  "annuity": "1122.95",
  "term": "60",
  "interest-rate": "4.66"
}
```

## applications/{application-number}/customer
Save customer data from onboarding screens.
Example of payload:

```json
{
  "id-number": "12345",
  "id-authority": "MUP Beograd",
  "id-valid-from": "2007-12-22T00:00:00Z",
  "id-valid-to": "2027-12-22T00:00:00Z",
  "content-urls": "",
  "country-resident": "Srbija",
  "city-resident": "Beograd",
  "county-resident": "Zvezdara",
  "postal-code-resident": "11000",
  "street-name-resident": "Pere Velimirovića",
  "street-number-resident": "10",
  "country-correspondent": "Srbija",
  "city-correspondent": "Beograd",
  "county-correspondent": "Zvezdara",
  "postal-code-correspondent": "11000",
  "street-name-correspondent": "Jovanke Radaković",
  "street-number-correspondent": "6",
  "account-owner": true,
  "related-customers": false,
  "politically-exposed-person": true,
  "influence-group": false,
  "bank-affiliated": false,
  "is-american-citizen": false
}
```
