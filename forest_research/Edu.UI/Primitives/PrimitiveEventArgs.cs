using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Edu.UI.Primitives
{
    /// <summary>
    /// Класс содержит данные о событиии 
    /// графических примитиввов
    /// </summary>
    public class PrimitiveEventArgs: EventArgs
    {
        /// <summary>
        /// Конструктор с параметом
        /// </summary>
        /// <param name="primitive">графический примитив</param>
        /// <returns></returns>
        public PrimitiveEventArgs(Primitive primitive)
            : base()
        {
            _primitive = primitive;
        }

        /// <summary>
        /// графический примитив
        /// </summary>
        private Primitive _primitive;

        /// <summary>
        /// Свойство возврящает графический примитив
        /// </summary>
        public Primitive Primitive
        {
            get 
            {
                return _primitive;
            }
        }
    }
}
