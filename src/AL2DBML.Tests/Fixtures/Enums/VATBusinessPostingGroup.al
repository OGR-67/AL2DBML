enum 50004 "VAT Business Posting Group" implements "IVATPostable", "IPostingGroup"
{
    Caption = 'VAT Business Posting Group';

    value(0; " ")
    {
        Caption = ' ';
    }
    value(1; "Domestic")
    {
        Caption = 'Domestic';
    }
    value(2; "EU")
    {
        Caption = 'EU';
    }
    value(3; "Export")
    {
        Caption = 'Export';
    }
}
