/// <summary>
/// 웨이브 스폰 시작을 외부에서 제어하기 위한 인터페이스.
/// PvP 대전 씬에서 PvpBattleController 가 핸드셰이크 완료 후 BeginWaves() 를 호출한다.
/// 일반(PvE) 씬에서는 autoStart=true 로 두면 기존처럼 Start() 에서 자동 시작.
/// </summary>
public interface IWaveDriver
{
    void BeginWaves();
    int CurrentWave { get; }
}
