table 18 "Customer"
{
    Caption = 'Customer';
    LookupPageID = "Customer List";
    DrillDownPageID = "Customer List";
    DataClassification = ToBeClassified;

    fields
    {
        field(1; "No."; Code[20])
        {
            Caption = 'No.';
            ToolTip = 'Specifies the unique customer identifier';
        }
        field(2; "Name"; Text[100])
        {
            Caption = 'Name';
            ToolTip = 'Specifies the customer company name';
        }
        field(3; "Search Name"; Code[100])
        {
            Caption = 'Search Name';
            ToolTip = 'Specifies an alternate name for searching';
        }
        field(4; "Name 2"; Text[100])
        {
            Caption = 'Name 2';
            ToolTip = 'Specifies an additional name line';
        }
        field(5; "Address"; Text[100])
        {
            Caption = 'Address';
            ToolTip = 'Specifies the street address';
        }
        field(6; "Address 2"; Text[100])
        {
            Caption = 'Address 2';
            ToolTip = 'Specifies an additional address line';
        }
        field(7; "City"; Text[30])
        {
            Caption = 'City';
            ToolTip = 'Specifies the city name';
        }
        field(8; "Post Code"; Code[20])
        {
            Caption = 'Post Code';
            ToolTip = 'Specifies the postal code';
        }
        field(9; "Country/Region Code"; Code[10])
        {
            Caption = 'Country/Region Code';
            ToolTip = 'Specifies the country or region';
        }
        field(10; "Phone No."; Text[20])
        {
            Caption = 'Phone No.';
            ToolTip = 'Specifies the customer telephone number';
        }
        field(11; "E-Mail"; Text[100])
        {
            Caption = 'E-Mail';
            ToolTip = 'Specifies the customer email address';
        }
        field(12; "Contact"; Text[100])
        {
            Caption = 'Contact';
            ToolTip = 'Specifies the name of the contact person';
        }
        field(13; "Balance"; Decimal)
        {
            Caption = 'Balance';
            ToolTip = 'Specifies the customer balance';
            FieldClass = FlowField;
            CalcFormula = sum("Cust. Ledger Entry"."Amount" where("Customer No." = field("No.")));
        }
        field(14; "Credit Limit (LCY)"; Decimal)
        {
            Caption = 'Credit Limit (LCY)';
            ToolTip = 'Specifies the maximum credit amount for the customer';
        }
        field(15; "Customer Posting Group"; Code[20])
        {
            Caption = 'Customer Posting Group';
            ToolTip = 'Specifies the posting group for general ledger';
        }
        field(16; "Currency Code"; Code[10])
        {
            Caption = 'Currency Code';
            ToolTip = 'Specifies the currency for customer transactions';
        }
        field(17; "Blocked"; Enum "Customer Blocked")
        {
            Caption = 'Blocked';
            ToolTip = 'Specifies if the customer is blocked';
        }
        field(18; "Created Date"; Date)
        {
            Caption = 'Created Date';
            ToolTip = 'Specifies when the customer record was created';
            Editable = false;
        }
        field(19; "Last Modified Date"; DateTime)
        {
            Caption = 'Last Modified Date';
            ToolTip = 'Specifies when the customer was last modified';
            Editable = false;
        }
    }

    keys
    {
        key(PK; "No.")
        {
            Clustered = true;
        }
        key(SK1; "Search Name")
        {
        }
        key(SK2; "Post Code")
        {
        }
        key(SK3; "E-Mail")
        {
        }
    }

    trigger OnInsert()
    begin
        "Created Date" := Today();
        ValidateCustomerData();
    end;

    trigger OnModify()
    begin
        "Last Modified Date" := CurrentDateTime();
        ValidateCustomerData();
        UpdateSearchName();
    end;

    trigger OnDelete()
    begin
        ValidateCustomerHasNoTransactions();
    end;

    trigger OnRename()
    begin
        "Last Modified Date" := CurrentDateTime();
    end;

    local procedure ValidateCustomerData()
    begin
        if "Name" = '' then
            Error('Customer name cannot be empty');

        if "Credit Limit (LCY)" < 0 then
            Error('Credit limit cannot be negative');
    end;

    local procedure ValidateCustomerHasNoTransactions()
    var
        CustLedgerEntry: Record "Cust. Ledger Entry";
    begin
        CustLedgerEntry.SetRange("Customer No.", "No.");
        if not CustLedgerEntry.IsEmpty() then
            Error('Cannot delete customer with existing transactions');
    end;

    local procedure UpdateSearchName()
    begin
        "Search Name" := "Name";
        if "Name 2" <> '' then
            "Search Name" += ' ' + "Name 2";
    end;
}
