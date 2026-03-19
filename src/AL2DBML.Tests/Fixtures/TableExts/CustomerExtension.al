tableextension 50000 "Customer Extension" extends "Customer"
{
    fields
    {
        field(50001; "Customer Category"; Code[20])
        {
            Caption = 'Customer Category';
            ToolTip = 'Specifies the customer category for segmentation';
        }
        field(50002; "Tax ID"; Text[20])
        {
            Caption = 'Tax ID';
            ToolTip = 'Specifies the customer tax identification number';
        }
        field(50003; "Preferred Contact Method"; Option)
        {
            Caption = 'Preferred Contact Method';
            OptionMembers = Email,Phone,Mail,Other;
            ToolTip = 'Specifies the preferred method to contact the customer';
        }
        field(50004; "InvoiceAmount"; Decimal)
        {
            Caption = 'Invoice Amount';
            FieldClass = FlowField;
            CalcFormula = sum("Customer Ledger Entry"."Amount" where("Customer No." = field("No."), "Document Type" = const(Invoice)));
        }
    }

    trigger OnAfterInsert()
    begin
        if "Customer Category" = '' then
            "Customer Category" := 'GENERAL';
    end;

    local procedure ValidateTaxID()
    begin
        if "Tax ID" <> '' and StrLen("Tax ID") < 5 then
            Error('Tax ID must be at least 5 characters');
    end;
}
