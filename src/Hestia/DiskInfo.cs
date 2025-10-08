namespace Hestia;

internal record DiskInfo(
    string DiskPath,
    string MapperPath,
    string LuksUuid
) {

    public bool IsUnlocked => !string.IsNullOrEmpty(MapperPath);

}
