namespace ProyectoIdentity.Common.Responses
{
    public class GenericResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
        public List<T> ListResults { get; set; }
    }
}
