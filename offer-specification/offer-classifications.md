---
visibility: public
---
Offer API Classifications
========

## [Cancelation Reasons](cancelation-reasons)
--------------

This classification represents customer's reason to cancel active application.

Literal                  | Code  | Description
-------------------------|-------|------------------------
`competitor-won`         | 0     | Competitor Won
`customer-second-thought`| 1     | Customer Second Thought
`other`                  | 2     | Other

## [Document Kinds](#document-kinds)
--------------

Specific purpose of document.

Literal                                     | Code  | Description
--------------------------------------------|-------|------------------------
`application`                               | 0     | Request for specific bank product
`contract`                                  | 1     | Contract with term and conditions of using bank product
`instruction`                               | 2     | Instruction
`statement`                                 | 3     | Statement
`installment-plan`                          | 4     | Installment plan
`employment-certificate`                    | 5     | Employment certificate
`business-register-excerpt`                 | 6     | Business register excerpt
`transaction-certificate`                   | 7     | Transaction certificate
`product-application-acceptance-phase checklist`| 8     | Application acceptance phase checklist
`product-application-creation-phase-checklist`  | 9     | Application creation phase checklist
`product-application-approval-phase-checklist`  | 10    | Application approval phase checklist
`product-application-activation-phase-checklist`| 11    | Application activation phase checklist
`withdraw-consent-for-credit-bureau-report`     | 12    | Withdraw consent for credit bureau report

## [Installment Activity Kinds](#installment-activity-kinds)
--------------

This classification represents status of request.

Literal                 | Code  | Description
------------------------|-------|------------------------
`fee-repayment`         | 0     | Fee repayment
`interest-repayment`    | 1     | Interest repayment
`principal-repayment`   | 2     | Principal repayment

## [Loan Kinds](#loan-kinds)
-----

This classification represents loan types.

Literal           | Code  | Description
------------------|-------|------------------------
`loan`            |0      | Loan
`credit-card`     |1      | Credit card
`guarantee`       |2      | Guarantee
`letter-of-credit`|3      | Letter of credit
`overdraft`       |4      | Overdraft

## [Employment Position Categories](#employment-position-categories)
-----

This classification represents employment positions categories

Literal                   | Code  | Description
--------------------------|-------|------------------------
`worker`                  |0      | Worker
`farmer`                  |1      | Farmer
`low-manager`             |2      | Low manager
`middle-manager`          |3      | Middle manager
`high-manager`            |4      | High manager
`executive-manager`       |5      | Executive manager
`office-employee`         |6      | Office employee
`government-employee`     |7      | Government employee
`teaching-employee`       |8      | Teaching employee
`medical-employee`        |9      | Medical employee

## [Education Levels](#education-levels)
-----

Literal                | Code  | Description
-----------------------|-------|----------------
`no-formal-education`  |0      | No school
`primary`              |1      | Elementary school
`lower-secondary`      |2      | III degree
`upper-secondary`      |3      | High school
`bachelor-degree`      |4      | College
`master-degree`        |5      | Master degree
`doctorate`            |6      | Doctorate
`bachelor`             |7      | Bachelor
`not-disclosed`        |8      | Not disclosed

## [Employment Kinds](#employment-kinds)
-----

Literal           | Code  | Description
------------------|-------|-----------------
`permanent`       |0      | Permanent
`temporary`       |1      | Temporary
`pensioner`       |2      | Pensioner
`selfEmployed`    |3      | Self employed
`unemployed`      |4      | Unemployed

## [Employer Kinds](#employer-kinds)
-----

Literal                         | Code  | Description
--------------------------------|-------|-----------------
`budgetary-and-public`          |0      | Budgetary and public
`enterpreneur-employer-kind`    |1      | Enterpreneur employer kind
`join-stock-company`            |2      | Join stock company
`limeted-liability-company`     |3      | Limited liability company
`other-employer-kind`           |4      | Other employer kind

[Expense Sources](#expense-sources)
------

Literal           | Description
------------------|------------------------
`rent`            | Rent
`alimony`         | Alimony
`loans`           | Loans
`other-deductions`      | Other Deductions

Offer API Enumerations
===============

## [Income Sources](#income-sources)
------

Literal           | Description
------------------|------------------------
`salary`            | Salary
`pension`           | Pension
`contract`          | Contract
`rent`              | Rent
`alimony`           | Alimony
`royalty`           | Royalty
`board-membership`  | Board Membership
`separation-allowance` | Separation Allowance

## [Application Statuses](#application-statuses)
------

Literal              | Description
---------------------|------------------------
`draft`    | Draft
`accepted` | Accepted
`active`   | Active
`rejected` | Rejected
`canceled` | Canceled
`approved` | Approved
`expired`  | Expired
`completed`| Completed

## [Arrangement Request Kinds](#arrangement-request-kinds)
-----

Literal                 | Description
------------------------|------------------------
`current-account`         | Current Account
`demand-deposit`          | Demand Deposit
`term-deposit`            | Term Deposit
`term-loan`               | Term Loan
`credit-facility`         | Credit Facility
`overdraft-facility`      | Overdraft Facility
`credit-card-facility`    | Credit Card Facility
`credit-line`             | Credit Line
`electronic-access-arrangement`| Electronic Access Arrangement
`card-access-arrangement`      | Card Access Arrangement
`securities-arrangement`       | Securities Arrangement
`other-product-arrangement`    | Other Product Arrangement


## [Document Statuses](#document-status)
------

Literal               | Description
----------------------|------------------------
`empty`               | Document is empty
`composed`            | Document is composed from template
`uploaded`            | Document is uploaded
`signed`              | Document is signed
`accepted-by-customer`| Document is accepted by customer

## [Document Origin](#document-origin)
------

Literal               | Description
----------------------|------------------------
`product`             | Document is defined in product requirements
`process`             | Document is added through business process
`agent`               | Agent added the document

## [Documen Context Kind](#document-context-kind)
------

Literal               | Description
----------------------|------------------------
`application`         | Application context
`party`               | Party context
`collateral`          | Collateral context
`arrangement-request` | Arrangement request context


## [Signing Options](#signing-options)
-------

Indicates how and where customer chose to sign agreement documents

Literal           | Description
------------------|------------------------
`branch`          | In the branch
`courier`         | Courier
`online`          | Online


## [Party Roles](#party-roles)
-------

Literal                  | Description
-------------------------|------------------------
`customer`               | Customer
`co-debtor`              | Co-debtor
`customer-representative`| Customer Representative
`guarantor`              | Guarantor
`authorized-person`      | Authorized Person
`other`                  | Other



## [Individual Statuses](#individual-status)
------

Literal              | Description
---------------------|------------------------
`living`             | Living
`deceased`           | deceased


## [Home Ownerships](#home-ownerships)
-----

Literal               | Description
----------------------|------------------------
`owns`                | Owns
`rents`               | Rents
`lives-with-relatives`| Lives With Relatives
`not-disclosed`       | Not Disclosed


## [Car Ownerships](#car-ownerships)
-----

Literal           | Description
------------------|------------------------
`owns`            | Owns
`does-not-own`    | Does Not Own
`not-disclosed`   | Not Disclosed

## [Employment Status](#employment-status)
-----

Literal           | Description
------------------|------------------------
`employed`        | Employed
`not-employed`    | Not Employed
`retired`         | Retired
`shop-owner`      | Shop Owner
`farmer`          | Farmer
`leave-of-absence`| Leave Of Absence
`part-time-job`   | Part Time Job


## [Rollover Options](#rollover-options)
-----

Literal                 | Description
------------------------|----------------
`not-allowed`           | Rollover is not allowed
`allowed-on-request`    | Allowed On Request
`automatic`             | Automatic


## [Interest Capitalization On Rollover](#interest-capitalization-on-rollover)
-----

Literal                 | Description
------------------------|----------------
`not-allowed`           | Not allowed
`allowed-on-request`    | Allowed On Request
`automatic`             | Automatic
