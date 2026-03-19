enum 50003 "Payment Terms Calc. Type" implements "IPaymentCalc"
{
    Caption = 'Payment Terms Calc. Type';

    value(0; " ")
    {
        Caption = ' ';
    }
    value(1; "Fixed Day")
    {
        Caption = 'Fixed Day';
    }
    value(2; "Current Month")
    {
        Caption = 'Current Month';
    }
}
