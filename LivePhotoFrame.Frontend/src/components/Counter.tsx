import { useState } from 'react';

function Counter() {
  const [count, setCount] = useState(0);

  return (
    <div>
      <h1>Counter</h1>
      <p>This is a simple example of a React component.</p>
      <p>Current count: <strong>{count}</strong></p>
      <button onClick={() => setCount(c => c + 1)}>Increment</button>
    </div>
  );
}

export default Counter;
