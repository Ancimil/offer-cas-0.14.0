
# Guide for transition from Offer API v1 to Offer API v2


## [General introduction](#general-introduction)

In Offer API v2 application represents array of arrangement requests. Each arrangement request is related to one products. Bundled applications are realised with this array. Main product code is derivated to application only for information purposes.

Application kind concept in Offer API v2 is replaced with arrangement-kind which is derivated from product kind and represents combination of Offer v1 application kinds, loan kinds and deposit opening kinds. Detail mapping is given in following table:

[Mapping application kinds](#mapping-application-kinds)

 Application Kind (v1)   | Loan Kinds (v1)         | Deposit Opening Kinds| Arrangement Kind (v2) 
-------------------------|-------------------------|----------------------|----------------------
`current-account-opening`|                         |                      |`current-account`
`deposit-opening`        |                         |`sight-deposit`       |`demand-deposit` 
`deposit-opening`        |                         |`term-deposit`        |`term-deposit`
`deposit-opening`        |                         |`current-account`     |`current-account`
`loan-origination`       |`loan`                   |                      |`term-loan`             
`loan-origination`       |`credit-card`            |                      |`credit-card-facility`
`loan-origination`       |`guarantee`              |                      |not implemented yet
`loan-origination`       |`letter-of-credit`       |                      |not implemented yet
`loan-origination`       |`overdraft`              |                      |`overdraft-facility`     


## Paths

### POST /me/drafts

In Offer API v2 __application__ resource covers draft status too. Status of initialized application resource is by default set to __draft__.

Replacement enpoint: __POST /applications__

Request payload mapping is described in [draft to initiate-offer-command](#draft-to-initiate-offer-command) mapping.

Responese is application-number as unique identifier of application resource. 

### GET /me/drafts

Replacement enpoint: __GET /applications__ 

Request should be provided with following filters:

* statuses-param equal to `[draft]`
* ????? kako se postize me?

Response is list of application-view objects. Mapping between responses is given in [draft and application-view](#draft-to-application-view) section.

### GET /me/drafts/{draft-id}

Replacement enpoint: __GET /applications/{application-number}__ 

Request contains application-number instead of draft-id

Response returns application object instead of draft.  Mapping between responses is given in [draft and application
 section](#draft-to-application)


### DELETE /me/drafts/{draft-id}

This enpoint is not supported in Offer API v2.

### PUT /me/drafts/{draft-id}

Replacement enpoint: __PUT /applications/{application-number}__

Request payload mapping is described in [draft to application section](#draft-to-application).

### GET /applications

The same endpoint for getting a list of applications already exists in Offer API v2.

Request parameters are all supported.

Response returns list of application-view objects instead list of application objects. Mapping between responses is given in [application to application-view section](#application-to-application-view) 


### GET /applications/{application-number}

The same endpoint for getting specific applications already exists in Offer API v2.

Response returns application object.  Mapping between responses is given in [application to application
 section](#application-to-application) 


### GET /applications/{application-number}/documents

The same endpoint for getting a list of application documents already exists in Offer API v2.

Request parameters:
 * `application-number-param` is supported
 * `upload-status-param` is partialy supported with parameter `document-status-param`
 * `trim-param` is not supported
 * `include-param` is not supported

Response returns list of application-document objects instead list of application document objects. Mapping between responses is given in [document to application-document section](#document-to-application-document) 

### PUT /applications/{application-number}/documents

The same endpoint for adding additional documents to application also exists in Offer API v2, but it doesn't handle attachnments as it is in v1. This command in Offer API v2 only creates slot for additional document by naming convention (`{application-numer}/{document-name}`). Adding attachments on content service is left to content service itself.


### POST /applications/{application-number}/cancel

The same endpoint for canceling application also exists in Offer API v2.

Request parameter `cancel-application-command` has following properties:
* `cancelation-reason` is supported
* `kind` is not supported. Whole application and all arrangement requests are canceled.

### PUT /applications/{application-number}/signing-option

This enpoint is not supported in Offer API v2. Instead PUT /applications/{application-number} should be used.

### POST /applications/deposit-openings

Replacement enpoint: __POST /applications__

Product kind should be one of: 
* `current-account-product`
* `demand-deposit`
* `term-deposit`* 
Request payload mapping is described in [initiate-deposit-opening-command to initiate-offer-command](#initiate-deposit-opening-command-to-initiate-offer-command) mapping.

### POST /applications/deposit-openings/validate

This enpoint is not supported in Offer API v2.

### POST /applications/deposit-openings/{application-number}/accept

Replacement enpoint: __POST /applications/{application-number}/accept__

accept-deposit-offer-command is not implemented. Accepted amount is the same as it is on arrangement request, as well as kind.

### POST /applications/loan-originations

Replacement enpoint: __POST /applications__

Product kind should be one of: 
* `term-loan`
* `credit-card-facility`
* `overdraft-facility` 
Request payload mapping is described in [initiate-loan-origination-command to initiate-offer-command](#initiate-loan-origination-command-to-initiate-offer-command) mapping.

### POST /applications/loan-originations/{application-number}/accept

Replacement enpoint: __POST /applications/{application-number}/accept__

accept-lending-offer-command is not implemented. Accepted amount is the same as it is on arrangement request, as well as kind.

## Object mappings

### [draft to initiate-offer-command](#draft-to-initiate-offer-command)

`draft` properties (v1)  | `initiate-offer-command` properties (v2)| Note
-------------------------|-----------------------------------------|------------
draft-id                 |                                         |There is no need to forward this value. Offer API v2 generate unique identifier for application (__application-number__) and provide it as response value. 
application-number       |application-number                       |Read Only. Unique identifier for application that Offer API v2 generate during application creation
kind                     |                                         |Read Only. Derivated from product kind.
status                   |status                                   |Read Only. In this command automatically set to 'draft'
parent-application-number|????                                     |
customer-number          |customer-number                          |
customer-name            |????                                     |
product-code             |product-code                             |
product-name             |product-name                             |
channel-code             |channel                                  |
reserved-account-number  |?????                                    |
signing-option           |?????                                    |
created                  |                                         |Read Only.
request-date             |                                         |Read Only.
expiration-date          |                                         |Read Only.
createdstatus-changed    |                                         |Read Only.
last-modified            |                                         |Read Only.
created-by-name          |                                         |Read Only.
comments                 |                                         |Comments are supported as separate service and not part of the Offer API v2
workitem-phase           |?????                                    |
negotiable               |?????                                    |
deposit-terms            |                                         |Refered to [deposit-terms mapping](#deposit-terms-to-arrangement-request-conditions)
bundled-applications     |?????                                    |
editing-step             |?????                                    |


### [draft to application-view](#draft-to-application-view)

`draft` properties (v1)  | `application-view` properties (v2) | Note
-------------------------|------------------------------------|------------
draft-id                 |                  |Not supported in Offer API v2. Instead unique identifier for application is __application-number__.
application-number       |application-number|
kind                     |arrangement-kind  |Reffered to [application kinds mapping](#mapping-application-kinds).
status                   |status            |
parent-application-number|????              |
customer-number          |customer-number   |
customer-name            |customer-name     |
product-code             |product-code      |
product-name             |product-name      |
channel-code             |channel           |
reserved-account-number  |?????             |
signing-option           |signing-option    |
created                  |created           |
request-date             |request-date      |
expiration-date          |expiration-date   |
created-status-changed   |status-changed    |
last-modified            |last-modified     |
created-by-name          |created-by-name   |
comments                 |                  |Comments are supported as separate service and not part of the Offer API v2
workitem-phase           |?????             |
negotiable               |?????             |
deposit-terms            |                  |Conditions are not displayed as part of application view object
bundled-applications     |?????             |
editing-step             |?????             |


### [draft to application](#draft-to-application)

`draft` properties (v1)  | `application` properties (v2) | Note
-------------------------|-------------------------------|------------
draft-id                 |                               |Not supported in Offer API v2. Instead unique identifier for application is __application-number__.
application-number       |application-number             |
kind                     |arrangement-kind               |Reffered to [application kinds mapping](#mapping-application-kinds).
status                   |status                         |
parent-application-number|????                           |
customer-number          |customer-number                |
customer-name            |customer-name                  |
product-code             |product-code                   |
product-name             |product-name                   |
channel-code             |channel                        |
reserved-account-number  |?????                          |
signing-option           |signing-option                 |
created                  |created                        |
request-date             |request-date                   |
expiration-date          |expiration-date                |
created-status-changed   |status-changed                 |
last-modified            |last-modified                  |
created-by-name          |created-by-name                |
comments                 |                               |Comments are supported as separate service and not part of the Offer API v2
workitem-phase           |?????                          |
negotiable               |?????                          |
deposit-terms            |                               |Refered to [deposit-terms mapping](#deposit-terms-to-arrangement-request-conditions)
bundled-applications     |?????                          |
editing-step             |?????                          |


### [application to application-view](#application-to-application-view)

`application` properties (v1)  | `application-view` properties (v2) | Note
-------------------------|------------------------------------|------------
application-number       |application-number|
kind                     |arrangement-kind  |Reffered to [application kinds mapping](#mapping-application-kinds).
status                   |status            |
parent-application-number|????              |
customer-number          |customer-number   |
customer-name            |customer-name     |
product-code             |product-code      |
product-name             |product-name      |
channel-code             |channel           |
reserved-account-number  |?????             |
signing-option           |signing-option    |
created                  |created           |
request-date             |request-date      |
expiration-date          |expiration-date   |
created-status-changed   |status-changed    |
last-modified            |last-modified     |
created-by-name          |created-by-name   |
comments                 |                  |Comments are supported as separate service and not part of the Offer API v2
workitem-phase           |?????             |
negotiable               |?????             |
deposit-terms            |                  |Conditions are not displayed as part of application view object

### [application to application](#application-to-application)

`application` properties (v1)  | `application` properties (v2) | Note
-------------------------|-------------------------------|------------
application-number       |application-number             |
kind                     |arrangement-kind               |Reffered to [application kinds mapping](#mapping-application-kinds).
status                   |status                         |
parent-application-number|????                           |Objasniti da je ovo obsolete
customer-number          |customer-number                |
customer-name            |customer-name                  |
product-code             |product-code                   |
product-name             |product-name                   |
channel-code             |channel                        |
reserved-account-number  |?????                          |dodati u v2
signing-option           |signing-option                 |
created                  |created                        |
request-date             |request-date                   |
expiration-date          |expiration-date                |
status-changed           |status-changed                 |
last-modified            |last-modified                  |
created-by-name          |created-by-name                |
comments                 |                               |Comments are supported as separate service and not part of the Offer API v2
workitem-phase           |?????                          |dodati u v2
negotiable               |?????                          |preferential-price
deposit-terms            |                               |Refered to [deposit-terms mapping](#deposit-terms-to-arrangement-request-conditions)


### [application to initiate-offer-command](#application-to-initiate-offer-command)

`application` properties (v1)  | `initiate-offer-command` properties (v2)| Note
-------------------------|-----------------------------------------|------------
application-number       |application-number                       |Read Only. Unique identifier for application that Offer API v2 generate during application creation
kind                     |                                         |Read Only. Derivated from product kind.
status                   |status                                   |Read Only. In this command automatically set to 'draft'
parent-application-number|????                                     |
customer-number          |customer-number                          |
customer-name            |????                                     |
product-code             |product-code                             |
product-name             |product-name                             |
channel-code             |channel                                  |
reserved-account-number  |?????                                    |
signing-option           |?????                                    |
created                  |                                         |Read Only.
request-date             |                                         |Read Only.
expiration-date          |                                         |Read Only.
createdstatus-changed    |                                         |Read Only.
last-modified            |                                         |Read Only.
created-by-name          |                                         |Read Only.
comments                 |                                         |Comments are supported as separate service and not part of the Offer API v2
workitem-phase           |?????                                    |
negotiable               |?????                                    |
deposit-terms            |                                         |Refered to [deposit-terms mapping]

### [deposit-terms to arrangement-request-conditions](#deposit-terms-to-arrangement-request-conditions)

`deposit-terms` properties (v1)| `arrangement-request` properties (v2)                        |Note
-------------------------------|--------------------------------------------------------------|--------------------
principal-amount               |amount                                                        |Amount is type if currency
term                           |term                                                          |
nominal-interest-rate          |napr                                                          |
interest-calculation-method    |arrangement-request-conditions.interest-rates[].is-compound   |
interest-calculation-basis     |arrangement-request-conditions.interest-rates[].calendar-basis|
efective-interest-rate         |eapr                                                          |
tax-rate                       |?????                                                         |
interest-amount                |?????                                                         |
tax-amount                     |?????                                                         |
payout-amount                  |?????                                                         |




### [document to application-document](#document-to-application-document)

`document` properties (v1)     | `application-document` properties (v2)  |Note
-------------------------------|-----------------------------------------|--------------------
product-code                   |                                         |arrangement-request-id uniquely determines product that document is related to
work-item-phase                |                                         |Not supported
document-kind                  |document-kind                            |
is-mandatory                   |is-mandatory                             |
is-printable                   |                                         |Not supported
is-uploadable                  |is-for-upload                            |
template-name                  |                                         |Not supported
template-uri                   |template-url                             |Not supported
attachment-uri                 |                                         |Composition of application-number and document name represents URL to content service folder where all attachments for that document is placed.
attachment-id                  |                                         |Not supported
upload-status                  |                                         |Partialy supported with document status
verification-comment           |                                         |Not supported
last-uploaded                  |                                         |Not supported

        
### [initiate-deposit-opening-command to initiate-offer-command](#initiate-deposit-opening-command-to-initiate-offer-command)

`initiate-deposit-opening-command` properties (v1)  | `initiate-offer-command` properties (v2)| Note
-------------------------|-----------------------------------------|------------
deposit-opening-kind     |                                         |Read Only. Derivated from product kind.
amount                   |amount, currency                         |
customer-number          |customer-number                          |
term                     |term                                     |
primary-currency         |                                         |Not implemented yet
product-code             |product-code                             |
signing-option           |signing-option                           |
requested-account-number |                                         |Not implemented yet
nickname                 |nickname                                 |Not implemented
parent-arrangement-number|                                         |Implemented in mechanizm of array of arrangement requests
requested-activation-date|                                         |Not implemented yet
campaign-code            |                                         |Not implemented yet
parent-application-number|                                         |Implemented in mechanizm of array of arrangement requests
transfer-account-number  |                                         |Not implemented yet
interest-account-number  |                                         |Not implemented yet
interest-accrual-period  |                                         |Not implemented yet
capitalization-option    |                                         |Not implemented yet
rollover-option          |                                         |Not implemented yet
negotiable               |                                         |
     
### [initiate-loan-origination-command to initiate-offer-command](#initiate-loan-origination-command-to-initiate-offer-command)

`initiate-loan-origination-command` properties (v1)  | `initiate-offer-command` properties (v2)| Note
-------------------------|-----------------------------------------|------------
loan-origination-kind    |                                         |Read Only. Derivated from product kind.
requested-amount         |amount, currency                         |
downpayment-amout        |downpayment-amount                       |Currency is the same as amount
invoice-amount           |invoice-amount                           |Currency is the same as amount
product-code             |product-code                             |Currency is the same as amount
channel-code             |channel                                  |
term                     |term                                     |
requested-activation-date|                                         |Not implemented yet
repayment-account        |                                         |Implemented via arrangement-request.accounts (settlement-account)
loan-purposes            |                                         |Not implemented yet (payment instructions)
lending-terms            |conditions                               |
installment-plan         |                                         |It is olways calculated by service
is-refinancing           |                                         |Not implemented yet
signing-option           |signing-option                           |
credit-bureau-consent    |                                         |Not implemented yet
applicant                |customer-number                          |Not implemented on initiation. Can be updated via PUT /application/{application-id}/involved-parties/{party-id}
parent-application-number|                                         |Implemented in mechanizm of array of arrangement requests
attachments              |                                         |Implemented via arrangement-request.documents

### [applicant-info to party](#applicant-info to party)

`applicant-info` properties (v1)  | `party` properties (v2)| Note
-------------------------|-----------------------------------------|------------
customer-number          |customer-number                          |
mothers-maiden-name      |mothers-maiden-name                      |
marital-status           |marital-status                           |
highest-education        |education-level                          |
home-ownership           |home-ownership                           |
residence-from-date      |residential-status-date                  |
current-address          |residential-status-date                  |