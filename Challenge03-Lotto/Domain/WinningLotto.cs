namespace Challenge03_Lotto.Domain;

public class WinningLotto
{
    private readonly Lotto _lotto;
    private readonly int _bonusNumber;

    public WinningLotto(Lotto lotto, int bonusNumber)
    {
        _lotto = lotto;
        _bonusNumber = bonusNumber;
    }

    // 이 아래로 추가 기능 구현
}
