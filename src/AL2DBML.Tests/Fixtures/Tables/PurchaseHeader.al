table 38 "Purchase Header"
{
    Caption = 'Purchase Header';
    DataClassification = ToBeClassified;

    fields
    {
        field(1; "Document Type"; Enum "Purchase Document Type")
        {
            Caption = 'Document Type';
        }
        field(2; "No."; Code[20])
        {
            Caption = 'No.';
        }
        field(3; "Buy-from Vendor No."; Code[20])
        {
            Caption = 'Buy-from Vendor No.';
        }
        field(4; "Pay-to Vendor No."; Code[20])
        {
            Caption = 'Pay-to Vendor No.';
        }
        field(5; "Amount"; Decimal)
        {
            Caption = 'Amount';
        }
        field(6; "Amount Including VAT"; Decimal)
        {
            Caption = 'Amount Including VAT';
        }
        field(7; "Salesperson/Purchaser Code"; Code[10])
        {
            Caption = 'Salesperson/Purchaser Code';
        }
        field(8; "Outstanding Amount (LCY)"; Decimal)
        {
            Caption = 'Outstanding Amount (LCY)';
        }
    }

    keys
    {
        key(PK; "Document Type", "No.")
        {
            Clustered = true;
        }
        key(SK1; "Buy-from Vendor No.")
        {
        }
    }
}
