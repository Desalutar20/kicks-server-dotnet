namespace Application.Abstractions.Hashing;

public interface IHashingService
{
    Result<HashedPassword> Hash(Password password);
    bool Verify(Password password, HashedPassword hashedPassword);
}